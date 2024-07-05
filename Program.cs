using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;





namespace changeDots
{
    //特定フォルダーのjpg画像をドット化してコンソールに出力
    internal class Program
    {
        //フォルダーの指定
        static string folderPath = @"";
        static int elementNum = 0;
        static void Main(string[] args)
        {

            Console.SetBufferSize(240, 400);
            Console.SetWindowSize(240, 400);
            ArrayList imagePaths = getImgPathList(folderPath);


            if (elementNum == 0)
            {
                Console.WriteLine("画像が見つかりませんでした");
                return;
            }


            int index = selectImgPathIndex(imagePaths);

            //パス名取り出し
            string selectImagePath = getImgPath(imagePaths, index);

            //ブロックの格納リスト
            List<List<int>> blockValues = changeDots(selectImagePath);

            Console.WriteLine("エンターで表示");
            Console.WriteLine("文字サイズを小さくして、全画面表示にしてね！");
            Console.ReadLine();
            printDots(blockValues);

        }



        //与えられた色に対し、コンソールで表示できる色の中から一番近いものを選びそれに対応した数字を返す
        public static int FindClosestColor(int colorValue)
        {
            double minDistance = double.MaxValue;
            int closestColorIndex = -1;

            int[] paletteColors = new int[]
            {
                0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0,
                0x808080, 0x0000ff, 0x00ff00, 0x00ffff, 0xff0000, 0xff00ff, 0xffff00, 0xffffff
            };

            // 与えられた色と16色のそれぞれとの距離を計算し、最も近い色を見つける
            for (int i = 0; i < paletteColors.Length; i++)
            {
                int paletteColor = paletteColors[i];
                double distance = CalculateColorDistance(colorValue, paletteColor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColorIndex = i;
                }
            }

            return closestColorIndex;
        }


        // 色間の距離を計算する関数
        private static double CalculateColorDistance(int color1, int color2)
        {


            // 色のR、G、B成分をそれぞれ取得
            int r1 = (color1 >> 16) & 0xFF;
            int g1 = (color1 >> 8) & 0xFF;
            int b1 = color1 & 0xFF;

            int r2 = (color2 >> 16) & 0xFF;
            int g2 = (color2 >> 8) & 0xFF;
            int b2 = color2 & 0xFF;

            // ユークリッド距離を計算
            double deltaR = r1 - r2;
            double deltaG = g1 - g2;
            double deltaB = b1 - b2;
            return deltaR * deltaR + deltaG * deltaG + deltaB * deltaB;
        }



        //最大の約数を求める
        //100未満になる場合は100で返すよ
        static int MaxDivisor(int width, int blockNum)
        {
            int tmp = 240;
            for (int i = blockNum; i > tmp; i--)
            {
                if (width % i == 0)
                {
                    return i;
                }
            }
            if (tmp > blockNum)
            {
                return blockNum;
            }
            return tmp;
        }

        static ArrayList getImgPathList(string folderPath)
        {
            // フォルダー内の画像ファイルのパスを取得
            string[] imgExt = { "*.jpg", "*.png", "*.jpeg" };
            ArrayList imgPaths = new ArrayList();
            Boolean isSuccess = true;
            do
            {

                try
                {
                    //フォルダーパスの指定！
                    Console.Write("フォルダー名:");
                    folderPath += Console.ReadLine();
                    if (folderPath == @"")
                    {
                        isSuccess = false;
                    }
                    else
                    {

                        foreach (string ext in imgExt)
                        {

                            imgPaths.Add(Directory.GetFiles(folderPath, ext));
                            elementNum += Directory.GetFiles(folderPath, ext).Length;
                        }
                        isSuccess = true; 
                    }

                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("パス名を間違えて入力してます");
                    isSuccess = false;
                    folderPath = @"";
                }
                catch (Exception e)
                {
                    Console.WriteLine("予期しないエラーが発生しちゃった");

                    return null;
                }

            } while (!isSuccess);

            return imgPaths;
        }

        static void printPath(ArrayList imgPaths)
        {
            //一覧表示
            Console.WriteLine("ファイル一覧");
            int pathCon = 0;
            foreach (string[] imageExtPath in imgPaths)
            {

                foreach (var imagePath in imageExtPath)
                {

                    Console.WriteLine(pathCon + "：" + imagePath);
                    pathCon++;
                }
            }
        }

        static int selectImgPathIndex(ArrayList imgPaths)
        {
            int num = -1;

            do
            {
                printPath(imgPaths);
                // ユーザーに整数を入力してもらう
                Console.WriteLine("整数値を入力してください:");
                string input = Console.ReadLine();

                // 入力された文字列を整数に変換
                if (int.TryParse(input, out num))
                {
                    // 正しく整数に変換できた場合
                    Console.WriteLine("入力された整数は: " + num);

                    //変換した数字が不正な場合
                    if (0 > num || num >= elementNum)
                    {
                        Console.WriteLine("入力された数字が大きすぎる、または小さすぎます。");
                        num = -1;
                    }
                }
                else
                {
                    // 変換に失敗した場合
                    Console.WriteLine("無効な入力です。整数を入力してください。");
                }

            } while (0 > num || num >= elementNum);
            return num;
        }

        static string getImgPath(ArrayList imgPaths, int index)
        {
            int pathCont = 0;
            string path = "";
            foreach (string[] imageExtPath in imgPaths)
            {

                foreach (var imagePath in imageExtPath)
                {

                    if (pathCont == index)
                    {
                        path = imagePath;

                    }
                    pathCont++;
                }
            }
            return path;
        }

        static List<List<int>> changeDots(string imgPath)
        {
            //画像読み込み
            Bitmap image = new Bitmap(imgPath);

            //画像の縦横px取得
            int width = image.Width;
            int height = image.Height;

            //ブロック数用変数
            int blockNum = 1200;

            //ブロック数を縦横の短いほうに合わせてみる
            if (blockNum > width && blockNum > height)
            {

                if (width > height)
                {
                    blockNum = height;
                }
                else
                {
                    blockNum = width;
                }

            }

            //画像の横に対しての縦の長さを計算　よこたて！
            double ratio = (double)height / 4 / (width);


            //縦横それぞれのブロック数を計算
            int blockX = MaxDivisor(width, blockNum);

            blockNum = blockX;

            double tmp = ratio * (((double)height / (width) > 1) ? 1.5 : 1.5);
            tmp = (double)blockNum * tmp;
            int blockY = (int)((double)MaxDivisor(height, (int)tmp));


            //縦横一ブロック当たりのピクセル数を計算
            int block_pixel_width = width / blockX;
            int block_pixel_height = height / blockY;


            //ここまでのデバッグ用（好きに消してね）
            Console.WriteLine("横：" + blockX + "ブロック 縦：" + blockY + "ブロック");
            Console.WriteLine("ブロック横：" + block_pixel_width + "ピクセル ブロック縦：" + block_pixel_height);

            //ブロックの格納リスト
            List<List<int>> blockValues = new List<List<int>>();

            //以下ループ

            //縦のブロックの数繰り返す
            for (int y = 0; y < blockY; y++)
            {

                //この行用のリスト
                List<int> lineValues = new List<int>();

                //横のブロック数繰り返す
                for (int x = 0; x < blockX; x++)
                {
                    changeDotsLine(ref lineValues, image, block_pixel_width, block_pixel_height, x, y);


                }//横のブロック終了
                blockValues.Add(lineValues);

            }

            return blockValues;
        }

        static void changeDotsLine(ref List<int> lineValues, Bitmap image, int block_pixel_width, int block_pixel_height, int x, int y)
        {
            //色の合計
            int sumRed = 0, sumGreen = 0, sumBlue = 0;
            //1ブロックあたりのピクセル数を計算
            int sumBlockPixel = block_pixel_width * block_pixel_height;


            //ブロック内の縦のピクセル数繰り返す
            for (int pixelY = 0; pixelY < block_pixel_height; pixelY++)
            {


                //ブロック内の横のピクセル数繰り返す
                for (int pixelX = 0; pixelX < block_pixel_width; pixelX++)
                {

                    //今のピクセルの座標指定
                    int nowX = x * block_pixel_width + pixelX;
                    int nowY = y * block_pixel_height + pixelY;


                    //デバッグ用
                    //Console.WriteLine("x:" + nowX + " y:" + nowY);

                    // ピクセルの色をRGB値で取得
                    Color pixelColor = image.GetPixel(nowX, nowY);

                    sumRed += pixelColor.R;
                    sumGreen += pixelColor.G;
                    sumBlue += pixelColor.B;


                }//ブロック内の横終了

            }//ブロック内の縦終了


            //ブロックの色の平均を計算
            int avgRed = sumRed / sumBlockPixel;
            int avgGreen = sumGreen / sumBlockPixel;
            int avgBlue = sumBlue / sumBlockPixel;

            //平均色をintで表す
            int argColor = (avgRed << 16) | (avgGreen << 8) | avgBlue;


            //このブロックの色を行用のリストに入れる
            lineValues.Add(FindClosestColor(argColor));


        }


        static void printDots(List<List<int>> blockValues)
        {
            int cony = 0;
            int conx = 0;
            foreach (var list in blockValues)
            {
                conx = 0;
                cony++;
                foreach (var pixel in list)
                {
                    conx++;
                    Console.BackgroundColor = (ConsoleColor)pixel;
                    Thread.Yield();
                    Console.Write(" ");
                }
                Console.BackgroundColor = (ConsoleColor)0;
                Thread.Yield();
                Console.WriteLine();
            }
            Console.WriteLine("X " + conx);
            Console.WriteLine("Y " + cony);

            Console.ReadLine();
        }
    }
}