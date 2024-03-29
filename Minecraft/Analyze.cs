﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    static class Analyze
    {
        static public string[] _symbols = { "==", "||", "&&", "&", "|", "<", ">", "!=", "++", "--",
                                            "*", "/", "%", "!", "=", "+", "-", "{", "(", ".", ";",
                                            ":", "[", ",", "??" };

        static public string[] _functions = { "random", "print", "println", "toString", "abs" };

        static private string[] _banWords = { "Math", "System", "", "out", "IO", "int", "float",
                                              "double", "public", "static", "void", "main", "String",
                                              "args", "Random", "Color", "Font", "File", "Timer" };

        static public string[] _operators = { "for", "if", "switch", "case", "do", "while", "break",
                                              "continue", "goto", "new", "random", "print", "println",
                                              "==", "||", "&&", "&", "|", "<", ">", "!=", "++", "--",
                                              "*", "/", "%", "!", "=", "+", "-", "{", "(", ".", ";",
                                              ":", "[", ",", "else", "toString", "??", "try", "catch",
                                              "finaly", "abs", "default"};

        static private int Count(string _str, string _sym)
        {
            if (_sym.Length > _str.Length)
                return 0;            

            int _counter = 0;
            
            for (int i = 0; i < _str.Length - _sym.Length; i++)          
                if (_str.Substring(i, _sym.Length) == _sym)
                    _counter++;

            return _counter;
        }

        static  private List<string> Vlogit(string code)
        {
            List<string> arrayFunc = new List<string>();

            string[] array = { "public", "static", "void", "private", "protected" };
            char[] list = { ' ', '\t', '\n', '=', '<', '>', '+', '-', '*', '/', '%', '(', ')', '!', ';', ':', '{', '}' };

            string word = "";
            string text = "";

            int scobka = 0;
            for (int i = 0; i < code.Length; i++)
            {
                if (!list.Contains(code[i]))  // собирает слова из символов
                    word += code[i];
                else if (word != "")
                {
                    if (array.Contains(word)) // находит начало функции
                    {
                        while (code[i] != '{')
                        {
                            text += code[i];                            
                            i++;
                        }
                        text += code[i];
                        if (text.IndexOf("class") != -1) // если это класс То все хуйня, давай по новой
                        {
                            text = "";
                            word = "";
                            continue;
                        }
                        scobka = 1;
                        i++;
                        while ((scobka > 0))
                        {
                            if (code[i] == '{')
                                scobka++;
                            if (code[i] == '}')
                                scobka--;
                            text += code[i];
                            i++;
                        }
                        arrayFunc.Add(text);
                        text = "";
                    }
                    text += word;
                    word = "";
                }
            }
            return arrayFunc;
        }

        static private string Refactoring(string _str)
        {
            _str = _str.Replace("\r", " ");
            _str = _str.Replace("\n", " ");
            _str = _str.Replace("\t", " ");            

            return _str;
        }
       
        static private string FindMethodName(string code)
        {
            string name = "";
            for (int i = code.IndexOf('('); ; i--)
                if (code[i] == ' ' && name != "")
                    break;
                else
                    name = code[i] + name;

            return (name.Trim() + ")");
        }

        static private string FindCode(string code)
        {            
            return Refactoring(code.Substring(code.IndexOf(')') + 1));
        }

        static public List<string> FindMethodNames(string _filePath)
        {
            var names = new List<string>();

            var code = Vlogit(System.IO.File.ReadAllText(_filePath));

            foreach (var item in code)
                names.Add(FindMethodName(item));

            return names;
        }

        static public List<Dictionary<string, int>> analyze(string _filePath)
        { 
            var code = Vlogit(System.IO.File.ReadAllText(_filePath));

            for (int i = 0; i < code.Count; i++)
                code[i] = Refactoring(code[i]);                                               

            var _listDictionary = new List<Dictionary<string, int>>();


            foreach (var itemG in code)            
            {
                string __code = FindCode(itemG);                

                var _dictionary = new Dictionary<string, int>();                
                
                _dictionary.Clear();

                _dictionary.Add(FindMethodName(itemG), -1);

                //
                // find all _symbols and replace them on space
                //

                foreach (var item in _symbols)
                    if (Count(__code, item) != 0)
                    {
                        _dictionary.Add(item, Count(__code, item));
                        __code = __code.Replace(item, " ");
                    }
                
                //
                // find strings and chars
                //           

                int flag = 0, tmp = 0;
                string word;

                for (int i = 0; i < __code.Length; i++)
                    if (__code[i] == '"' || __code[i] == '\'')
                    {
                        flag++;
                        if (flag == 2)
                        {
                            word = __code.Substring(tmp, i - tmp + 1);
                            if (!_dictionary.ContainsKey(word))
                                _dictionary.Add(word, 1);
                            else
                                _dictionary[word]++;
                            __code = __code.Remove(tmp, i - tmp + 1);
                            flag = 0;
                        }
                        tmp = i;
                    }

                // 
                // Create List with last words
                //

                __code = __code.Replace("}", " ");
                __code = __code.Replace("]", " ");
                __code = __code.Replace(")", " ");

                var _otherWords = new List<string>();

                foreach (var item in __code.Split(' '))
                    if (item != "")
                        _otherWords.Add(item);

                //
                // add "()" to functions and remove from global ()
                //

                for (int i = 0; i < _otherWords.Count; i++)
                    if (_functions.Contains(_otherWords[i]))                        
                        _dictionary["("]--;

                //
                //  ADD last world to dictionary
                //

                foreach (var item in _otherWords)
                    if (!_dictionary.ContainsKey(item))
                        _dictionary.Add(item, 1);
                    else
                        _dictionary[item]++;

                //
                //  do while
                //

                if (_dictionary.ContainsKey("while") && _dictionary.ContainsKey("do"))
                    _dictionary["while"] -= _dictionary["do"];

                if (_dictionary.ContainsKey("while"))
                    _dictionary["("] -= _dictionary["while"];

                if (_dictionary.ContainsKey("do"))
                    _dictionary["("] -= _dictionary["do"];

                //
                // if else
                //

                if (_dictionary.ContainsKey("if") && _dictionary.ContainsKey("else"))
                    _dictionary["if"] -= _dictionary["else"];                              

                //
                // try catch finaly
                //

                if (_dictionary.ContainsKey("try") && _dictionary.ContainsKey("catch"))
                    _dictionary["try"] -= _dictionary["catch"];

                if (_dictionary.ContainsKey("try") && _dictionary.ContainsKey("finaly"))
                    _dictionary["try"] -= _dictionary["finaly"];

                //
                //  Ban words from _banWords and remove all keys with value 0
                //

                foreach (var item in _banWords)
                    if (_dictionary.ContainsKey(item))
                        _dictionary.Remove(item);

                if (_dictionary.ContainsKey("("))
                    if (_dictionary["("] == 0)
                        _dictionary.Remove("(");

                if (_dictionary.ContainsKey("while"))
                    if (_dictionary["while"] == 0)
                        _dictionary.Remove("while");

                if (_dictionary.ContainsKey("if"))
                    if (_dictionary["if"] == 0)
                        _dictionary.Remove("if");

                if (_dictionary.ContainsKey("try"))
                    if (_dictionary["try"] == 0)
                        _dictionary.Remove("try");

                _listDictionary.Add(_dictionary);   
            }

            //
            // Make Global Dictionary with all Functions
            //

            var _globaldick = new Dictionary<string, int>();
            _globaldick.Add("GLOBAL", 0);
            foreach (var dictionary in _listDictionary)
                foreach (var key in dictionary.Keys)
                    if (!_globaldick.ContainsKey(key))
                        _globaldick.Add(key, dictionary[key]);
                    else
                        _globaldick[key] += dictionary[key];

            foreach (var methodName in FindMethodNames(_filePath))                
                _globaldick.Remove(methodName);

            _listDictionary.Add(_globaldick);

            return _listDictionary;
        }
    }
}
