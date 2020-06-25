using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WMIConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, string> keyMap = new Dictionary<string, string>() {
            { "sint16",     "int"   },
            { "boolean",    "bool"     },
            { "Boolean",    "bool"     },
            { "uint8",      "byte"    },
            { "uint16",     "UInt16"   },
            { "uint32",     "UInt32"   },
            { "uint64",     "UInt64"   },
            { "Real32",     "float"     },
            { "datetime", "DateTime?"},
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void convertBtn_Click(object sender, RoutedEventArgs e)
        {
            string convertedOutput = string.Empty;
            string wmiInput = wmiClassTextBox.Text;
            string[] allLines = Regex.Split(wmiInput, Environment.NewLine, RegexOptions.IgnoreCase);
            //String[] allLines = wmiInput.Split(Environment.NewLine);
            foreach (string iterStr in allLines)
            {
                if (iterStr.Trim() == "{" || iterStr.Trim() == "};")
                {
                    //do nothing.
                }
                else
                {
                    convertedOutput += Environment.NewLine;
                    convertedOutput += "[DataMember(IsRequired = false)]";
                    convertedOutput += Environment.NewLine;
                    convertedOutput += "public ";
                }
                string[] subStrArray = iterStr.Trim().Split(' ');
                string keyName = subStrArray[0].Trim();
                if (keyMap.ContainsKey(keyName))
                {
                    for (int i = 0; i < subStrArray.Length; ++i)
                    {
                        if (i == 0)
                        {
                            convertedOutput += keyMap[keyName];
                        }
                        else
                        {
                            convertedOutput += " ";
                            convertedOutput += subStrArray[i];
                        }
                    }
                    convertedOutput += Environment.NewLine;
                }
                else
                {
                    convertedOutput += iterStr;
                    convertedOutput += Environment.NewLine;
                }
            }

            convertedOutput += @"public void InitializeByWMIObj(ManagementObject queryObj)";
            convertedOutput += Environment.NewLine;
            convertedOutput += "{";
            convertedOutput += Environment.NewLine;
            convertedOutput += "object obj = null;";
            convertedOutput += Environment.NewLine;

            foreach (string iterStr in allLines)
            {
                if (iterStr.Trim() == "{" || iterStr.Trim() == "};" || iterStr.Trim().Contains("class Win32") || iterStr.Trim().Contains(@"UUID(""{"))
                {
                    //do nothing.
                }
                else
                {
                    string[] subStrArray = iterStr.Trim().Split(' ');
                    string fieldName = subStrArray[subStrArray.Length - 1];
                    fieldName = fieldName.Substring(0, fieldName.Length - 1);

                    string firstLine = string.Format(@"obj = queryObj.GetPropertyValue(""{0}"");", fieldName);
                    convertedOutput += firstLine;
                    convertedOutput += Environment.NewLine;
                    convertedOutput += "if (obj != null)";
                    convertedOutput += Environment.NewLine;
                    convertedOutput += "{";
                    convertedOutput += Environment.NewLine;

                    string secondLine = "";
                    if (iterStr.Contains("string"))
                    {
                        secondLine = string.Format(@"this.{0} = obj.ToString();", fieldName);
                    }
                    else if (iterStr.Contains("datetime"))
                    {
                        secondLine = string.Format(@"this.{0} = ManagementDateTimeConverter.ToDateTime(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("uint8") || iterStr.Contains("uint16"))
                    {
                        //this.ExecutionState = UInt16.Parse(obj.ToString());
                        secondLine = string.Format(@"this.{0} = UInt16.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("uint32"))
                    {
                        secondLine = string.Format(@"this.{0} = UInt32.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("uint64"))
                    {
                        secondLine = string.Format(@"this.{0} = UInt64.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("sint16"))
                    {
                        secondLine = string.Format(@"this.{0} = int.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("boolean") || iterStr.Contains("Boolean"))
                    {
                        secondLine = string.Format(@"this.{0} = bool.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("float"))
                    {
                        secondLine = string.Format(@"this.{0} = float.Parse(obj.ToString());", fieldName);
                    }
                    else if (iterStr.Contains("double"))
                    {
                        secondLine = string.Format(@"this.{0} = double.Parse(obj.ToString());", fieldName);
                    }

                    convertedOutput += secondLine;
                    convertedOutput += Environment.NewLine;
                    convertedOutput += "}";
                    convertedOutput += Environment.NewLine;
                }
                convertedOutput += Environment.NewLine;
            }
            //todo

            convertedOutput += "}";
            convertedOutput += Environment.NewLine;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>

                {

                    srcClassTextBox.Text = convertedOutput;

                }));

            //srcClassTextBox.Text = convertedOutput;
        }
    }
}
