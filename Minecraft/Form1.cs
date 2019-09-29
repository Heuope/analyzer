using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minecraft
{
    public partial class Form1 : Form
    {
        private string _FilePath;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LengthDick.Text = "Length      : 0\r\nDictionary : 0\r\nVolume     : 0";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //
            //  CHOISE FILE PATH
            //

            openFileDialog.InitialDirectory = @"C:\Users\konst\Desktop";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                _FilePath = openFileDialog.FileName;
            else
                _FilePath = null;            

            if (_FilePath != null)
            {
                comboBox1.Items.Clear();
                foreach (var item in Analyze.FindMethodNames(_FilePath))
                    comboBox1.Items.Add(item);
                if (comboBox1.Items.Count > 1)
                    comboBox1.Items.Add("GLOBAL");
            }
        }        

        private void ShowTable(string _methodName)
        {
            if (_FilePath == null)
                return;            

            var _listDict = Analyze.analyze(_FilePath);

            dataGridView1.Rows.Clear();

            int i = 0, j = 0, N1 = 0, N2 = 0, way = 0;

            foreach (var _dict in _listDict)
                if (_dict.ContainsKey(_methodName))
                {                    
                    _dict.Remove(_methodName);

                    int _size = 0;

                    foreach (var item in _dict.Keys)
                        if (Analyze._operators.Contains(item))                            
                            _size++;

                    if (_size >= _dict.Count / 2)
                        dataGridView1.Rows.Add(_size);
                    else
                        dataGridView1.Rows.Add(_dict.Count - _size);

                    i = 1; j = 1; N1 = 0; N2 = 0; way = 0;                     

                    //
                    // Shows operators
                    //

                    foreach (var _oper in Analyze._operators)
                        if (_dict.ContainsKey(_oper))
                        {
                            N1 += _dict[_oper];
                            dataGridView1[0, way].Value = i++;
                            if (_oper == "do")
                                dataGridView1[1, way].Value = _oper + "..while";
                            else if (Analyze._functions.Contains(_oper))
                                dataGridView1[1, way].Value = _oper + "()";
                            else if (_oper == "(")
                                dataGridView1[1, way].Value = _oper + ")";
                            else if (_oper == "{")
                                dataGridView1[1, way].Value = _oper + "}";
                            else if (_oper == "[")
                                dataGridView1[1, way].Value = _oper + "]";
                            else
                                dataGridView1[1, way].Value = _oper;

                            dataGridView1[2, way].Value = _dict[_oper];
                            way++;
                            _dict.Remove(_oper);
                        }

                    //
                    // Shows operands
                    //

                    way = 0;

                    foreach (var _item in _dict)
                    {
                        N2 += _item.Value;                        
                        dataGridView1[3, way].Value = j++;
                        dataGridView1[4, way].Value = _item.Key;
                        dataGridView1[5, way].Value = _item.Value;
                        way++;
                    }
                    break;
                }

            LengthDick.Text = 
                "Length     : " + (N1 + N2).ToString() + "\r\n" +
                "Dictionary : " + (i + j - 2).ToString() + "\r\n" +
                "Volume     : " + ((int)((N1 + N2) * Math.Log(i + j - 2, 2))).ToString();

        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (_FilePath != null)
                System.Diagnostics.Process.Start(_FilePath);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowTable(comboBox1.Text);      
        }
    }
}
