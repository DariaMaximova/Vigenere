using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace cripto
{
    class Program
    {
        public static char[] alphabet = { 'А','Б','В','Г','Д','Е', 'Ё','Ж','З','И',
                'Й','К','Л','М','Н','О','П','Р','С','Т','У','Ф','Х','Ц',
                'Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я' };

        public static string sourcePath = @"C:\Users\user\Desktop\crypto2.txt";
        public static string trigramPath = @"C:\Users\user\source\repos\cripto\cripto\russian_trigrams.txt";
        public static string TransformText(string path)
        {
            string oldText = "";
            string text = "";
            
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    oldText = sr.ReadToEnd();
                    oldText = oldText.ToUpper();


                    for (int i = 0; i < oldText.Length; i++)
                    {
                        if (char.IsLetter(oldText[i]))
                        {
                            text += oldText[i];
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return text;
        }
        public static string Encrypt(string text, string key, bool encrypt)
        {

            int keyInd; //индекс символа ключа 
            int letterPos; //позиция буквы в алфавите
            int newLetterPos; //новая позиция буквы после шифрования 
            string newText = ""; //зашифрованнный текст
            char newLetter; //зашифрованная буква
            int keySymPos; //позиция буквы ключа в алфавите
            
            for (int i = 0; i < text.Length; i++)
            {

                if (char.IsLetter(text[i]))
                {
                    keyInd = i % key.Length; // определение какой букве ключа соответсвует буква в тексте

                    if ('Ё' == text[i])
                    {
                        letterPos = 6;
                    }
                    else
                    {
                        letterPos = text[i] - 'А'; // ищем позицию в алфавите

                        if (letterPos > 5)
                        {
                            letterPos++;
                        }

                    }

                    if ('Ё' == key[keyInd])
                    {
                        keySymPos = 6;
                    }
                    else
                    {
                        keySymPos = key[keyInd] - 'А'; 
                        if (keySymPos > 5)
                        {
                            keySymPos++;
                        }

                    }
                    if (encrypt)
                    {
                        newLetterPos = (letterPos + keySymPos) % 33;
                    }
                    else
                    {
                        newLetterPos = (letterPos - keySymPos + 33) % 33;
                    }
                    newLetter = alphabet[newLetterPos];
                    newText += newLetter;
                }
            }

            return newText;
        }

        // находим длину ключа, используя метод индекса совпадений
        public static int CalculateKeyLen(string newText)
        {
            Dictionary<char, int> stats = new Dictionary<char, int>(); // определяем частоту встречи букв в тексте

            for (int i = 0; i < newText.Length; i++)
            {
                if (stats.ContainsKey(newText[i]))
                {
                    stats[newText[i]]++;
                }
                else
                {
                    stats[newText[i]] = 1;
                }
            }

            int length = -1;
            Dictionary<int, double> pairs = new Dictionary<int, double>(); // соответствие длины ключа и индекса совпадений при этой длине

            for (int len = 2; len < 10; len++) // перебираем все возможные длины ключа до 10 символов и рассчитываем индекс совпадений
            {
                ArrayList alph = new ArrayList(new char[] { 'А','Б','В','Г','Д','Е', 'Ё','Ж','З','И',
                'Й','К','Л','М','Н','О','П','Р','С','Т','У','Ф','Х','Ц',
                'Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я' }); 
                ArrayList alphabetList = new ArrayList();

                for (int k = 0; k < len; k++)
                {
                    ArrayList copy = new ArrayList();
                    foreach (char sym in alph)
                    {
                        copy.Add(sym);
                    }

                    alphabetList.Add(copy);
                }

                double[] icList = new double[len];

                for (int i = 0; i < newText.Length; i++)
                {

                    int chainInd = i % len;

                    if (((ArrayList)alphabetList[chainInd]).Contains(newText[i]))
                    {
                        icList[chainInd] += (double)stats[newText[i]] *
                           ((double)(stats[newText[i]] - 1));
                        ((ArrayList)alphabetList[chainInd]).Remove(newText[i]);
                    }
                }

                double ic = 0;

                for (int h = 0; h < len; h++)                 {
                    icList[h] = icList[h] / (newText.Length * (newText.Length - 1));
                    ic += icList[h];
                }

                ic = ic / len;

                length = len;
                pairs.Add(length, ic);


            }

            double minIC = 99999999;
            int lenIC = -1;

            foreach (var key in pairs.Keys) // выбираем минимальный индекс совпадений
            {

                if (pairs[key] < minIC)
                {
                    minIC = pairs[key];
                    lenIC = key;
                }
            }

            return lenIC;
        }
        public static string GetStartKey(int len)
        {
            string key = "";
            for (int i = 0; i < len; i++)
            {
                key += 'А';
            }
            return key;
        }
        public static string ChangeKey(string key, int pos) // меняем букву в ключе с позицией pos на следующую
        {
            int letterPosBefore = (int)(key[pos] - 'А');
            char[] newKey = key.ToCharArray();

            if (newKey[pos] == 'Е')
            {
                newKey[pos] = 'Ё';
            }
            else if (newKey[pos] == 'Ё')
            {
                newKey[pos] = 'Ж';
            }
            else
            {
                letterPosBefore++;
                if (letterPosBefore > 5)
                {
                    letterPosBefore++;
                }
                newKey[pos] = alphabet[letterPosBefore % 33];
            }
            return new string(newKey);
        }
        public static double FitnessText(string newText, string fileName)
        {
            Dictionary<string, double> ngram = new Dictionary<string, double>();
            int ngramCount = 0;

            string[] lines = File.ReadAllLines(fileName);


            for (int i = 0; i < lines.Length; i++)
            {
                string key;
                string value;
                string[] split = lines[i].Split(' ');

                key = split[0];
                value = split[1];

                key = key.ToUpper();
                ngram.Add(key, double.Parse(value));
                ngramCount += (int)ngram[key];
            }

            Dictionary<string, double> ngramProb = new Dictionary<string, double>();

            foreach (var key in ngram.Keys)
            {
                ngramProb.Add(key, Math.Log(ngram[key] / ngramCount));
            }

            double koeff = 0;

            for (int k = 0; k < (newText.Length - 2); k++)
            {
                string key = newText[k].ToString() + newText[k + 1].ToString() + newText[k + 2].ToString();
                if (ngramProb.ContainsKey(key))
                {
                    koeff += ngramProb[key];
                }
                else
                {
                    koeff += Math.Log(0.0001 / ngramCount);
                }

            }

            return koeff;
        }

        public static string CryptoAnalyze(string text, int len) //находим ключ для расшифровки текста по его длине len
        {
            string key = GetStartKey(len);
            string maxKey = key;
            double maxFit = -9999999;
            double currentFit;


            string decryptText = Encrypt(text, key, false);

            currentFit = FitnessText(decryptText, trigramPath);
            maxFit = currentFit;

            for (int k = 0; k < len; k++)
            {

                for (int i = 0; i < 32; i++)
                {
                    key = ChangeKey(key, k);
                    decryptText = Encrypt(text, key, false);

                    currentFit = FitnessText(decryptText, trigramPath);

                    if (currentFit > maxFit)
                    {
                        maxFit = currentFit;
                        maxKey = key;
                    }

                }

            }

            key = maxKey;
            for (int k = 0; k < len; k++)
            {
                key = maxKey;

                for (int i = 0; i < 32; i++)
                {
                    key = ChangeKey(key, k);
                    decryptText = Encrypt(text, key, false);

                    currentFit = FitnessText(decryptText, trigramPath);

                    if (currentFit > maxFit)
                    {
                        maxFit = currentFit;
                        maxKey = key;
                    }

                }

            }

            return maxKey;
        }

        static void Main(string[] args)
        {
            string sourcetext;
            sourcetext = TransformText(sourcePath);
            Console.WriteLine(sourcetext);
            Console.Write("Введите ключ: ");
            string key = Console.ReadLine();

            string encryptText = Encrypt(sourcetext, key, true);

            CalculateKeyLen(encryptText);

            Console.WriteLine("Длина текста: " + sourcetext.Length);
            Console.WriteLine();
            Console.WriteLine(encryptText);
            Console.WriteLine();
            Console.WriteLine("Длина ключа - " + CalculateKeyLen(encryptText));
            Console.WriteLine();

            string decryptText = Encrypt(encryptText, key, false);

            string foundKey = CryptoAnalyze(encryptText, CalculateKeyLen(encryptText));
            StreamWriter sw = new StreamWriter(@"C:\Users\user\source\repos\cripto\cripto\decrypted.txt");

            sw.WriteLine( foundKey);
           
            decryptText = Encrypt(encryptText, foundKey, false);

            sw.WriteLine(decryptText);

            Console.WriteLine("MAXKEY - " + foundKey);
            Console.WriteLine();
            Console.WriteLine(decryptText);
            Console.WriteLine();
            sw.Close();

            Console.ReadKey();
        }
    }
}
