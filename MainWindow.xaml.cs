//Faltan probar el grabado de lineas en el xml de logo y sello
//Encase_XS_WPFVersion
//Autor: Jose Vallejo Fernandez
//Grupo Epelsa
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.IO;

using System.ComponentModel;
using System.Reflection;
using System.Threading;

enum types_items { none = 0, lbl, field, logo, box, barcode, seal};
namespace Encase_XS_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    #region Función para el mapeo de imagenes
    internal static class ResourceAccessor
    {
        public static Uri Get(string resourcePath)
        {
            var uri = string.Format(
                "pack://application:,,,/{0};component/{1}"
                , Assembly.GetExecutingAssembly().GetName().Name
                , resourcePath
            );

            return new Uri(uri);
        }
    }
    #endregion
    public partial class MainWindow : Window
    {
        #region Clase Imagen
        Image im = new Image();
        #endregion
        #region Variables de objeto//Variables y listas relacionadas con los objetos que añadimos al canvas

        int lbl_cnt_weight = 0;
        int lbl_cnt_resume = 0;
        int lbl_single_select = 0;
        int seal_counter;
        struct label_item  //Estructura para dar valor a las propiedades de cada objeto
        {
            public Grid widget;
            public string orientation;
            public string font;     
            public int font_size;
            public string labelSelec;
            public int line_width;  
            public bool is_selected;
            public int weight_num;
            public int resume_num;
            public int id;
            public int idCount;
            public string barcode_type;
            public string condition;
            public string cond_resource;
            public string cond_value;
            public types_items type;
            public string textbox_type;
            public bool negrita;
            public bool cursiva;
            public string align;

        }
        
        //Lista para guardar los objetos creados
        List<label_item> m_lstLabels = new List<label_item>();
        
        #endregion
        #region OpenFile //Variables y declaraciones relacionadas con Abrir y guardar ficheros
        OpenFileDialog dlg = new OpenFileDialog();
        SaveFileDialog dlg_save = new SaveFileDialog();
        public void OpenFile_Click()
        {
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();            
        }
            #endregion
        #region Variables de Etiqueta //Variables relacionadas con la creación del canvas
        int m_ancho_Etiqueta;
        int m_alto_Etiqueta;
        string m_nombre_Etiqueta;
        int cnvControl = 0;
        string labelOrientation;
        #endregion
        #region Variables de Control //Variables relacionadas con el control
        bool size = false;
        bool drag = false;
        Point startPoint;
        #endregion
        #region MainWindow //Función Main
        public MainWindow()
        {            
            InitializeComponent();
            this.SetLanguageDictionary();
            
        }
        #endregion
        #region Funciónes selectoras del idioma 
        private void SetLanguageDictionaryManual(string language)
        {
            ResourceDictionary dict = new ResourceDictionary();
            
            switch (language)
            {
                case "es":
                    dict.Source = new Uri("..\\Resource\\language.es.xaml", UriKind.Relative);
                    break;
                case "en":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                case "de":
                    dict.Source = new Uri("..\\Resource\\language.de.xaml", UriKind.Relative);
                    break;
                case "it":
                    dict.Source = new Uri("..\\Resource\\language.it.xaml", UriKind.Relative);
                    break;
                case "fr":
                    dict.Source = new Uri("..\\Resource\\language.fr.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "es":
                    dict.Source = new Uri("..\\Resource\\language.es.xaml", UriKind.Relative);
                    break;
                case "es-ES":
                    dict.Source = new Uri("..\\Resource\\language.es.xaml", UriKind.Relative);
                    break;
                case "eu-ES":
                    dict.Source = new Uri("..\\Resource\\language.es.xaml", UriKind.Relative);
                    break;
                case "fr-FR":
                    dict.Source = new Uri("..\\Resource\\language.fr.xaml", UriKind.Relative);
                    break;
                case "de-DE":
                    dict.Source = new Uri("..\\Resource\\language.de.xaml", UriKind.Relative);
                    break;
                case "el-GR":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                case "it-IT":
                    dict.Source = new Uri("..\\Resource\\language.it.xaml", UriKind.Relative);
                    break;
                case "en":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                case "en-US":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                case "en-GB":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                case "pt-PT":
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resource\\language.en.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
        #endregion
        #region Crea etiqueta //Crea una etiqueta con los datos pasados en ventana de nueva etiqueta
        public void Crear_Etiqueta(string m_nombre_Etiqueta, int m_ancho_Etiqueta, int m_alto_Etiqueta)
        {
            //Limpia los datos de las etiquetas existentes
            cnv1.Children.Clear();
            cnv2.Children.Clear();
            m_lstLabels.Clear();
            lbl_cnt_weight = 0;
            lbl_cnt_resume = 0;
            //Da el tamaño al canvas introducido en la ventana de creación de etiqueta
            cnv1.Width = m_ancho_Etiqueta * 8;            
            cnv2.Width = m_ancho_Etiqueta * 8;
            cnv1.Height = m_alto_Etiqueta * 8;            
            cnv2.Height = m_alto_Etiqueta * 8;
            //Actualiza el Textbox de ancho/alto del documento con los datos introducidos
            Document_Height.Text = (cnv1.Height / 8).ToString();
            Document_Width.Text = (cnv1.Width / 8).ToString();
            //Pone el fondo del canvas de color blanco
            cnv1.Background = Brushes.White;
            cnv2.Background = Brushes.White;
            //Lo alinea a la izquierda
            cnv1.HorizontalAlignment = HorizontalAlignment.Left;
            cnv2.HorizontalAlignment = HorizontalAlignment.Left;
            //Le añade margenes
            cnv1.Margin.Equals(5);
            cnv2.Margin.Equals(5);
            cnv1.Margin.Top.Equals(5);
            cnv2.Margin.Top.Equals(5);
            //Actualiza la TextBox de nombre de documento con el nombre que le hayamos dado en la ventana
            Nombre_Tipo_Documento.Text = m_nombre_Etiqueta;
            //Cambia el tamaño de la regla y su valor máximo para que sea igual al del canvas
            ruler1h.Width = cnv1.Width + 6;
            ruler1v.Height = cnv1.Height + 6;          
            ruler2h.Width = cnv2.Width + 6;
            ruler2v.Height = cnv2.Height + 6;
            ruler1h.MaxValue = cnv1.Width/8;
            ruler1v.MaxValue = cnv1.Height/8;
            ruler2h.MaxValue = cnv2.Width/8;
            ruler2v.MaxValue = cnv2.Height/8;
            //Añade el canvas al StackPanel de diseño
            
            Design_Frame1.Children.Add(cnv1);
            
            Design_Frame2.Children.Add(cnv2);
            //Centra el foco por defecto en el Tab1 "WEIGHT"
            Tbt1.Focus();
            //Actualiza el valor de TextBox de Alto y ancho de documento
            
        }
        public void Crear_Etiqueta_Reader(string m_nombre_Etiqueta, int m_ancho_Etiqueta, int m_alto_Etiqueta, string l_strOrientation)
        {
            labelOrientation = "";
            labelOrientation = l_strOrientation;
            select_page_orientation(labelOrientation);
            //Da el tamaño al canvas introducido en la ventana de creación de etiqueta
            cnv1.Width = m_ancho_Etiqueta * 8;
            cnv2.Width = m_ancho_Etiqueta * 8;
            cnv1.Height = m_alto_Etiqueta * 8;
            cnv2.Height = m_alto_Etiqueta * 8;
            //Actualiza el Textbox de ancho/alto del documento con los datos introducidos
            Document_Height.Text = (cnv1.Height / 8).ToString();
            Document_Width.Text = (cnv1.Width / 8).ToString();
            //Pone el fondo del canvas de color blanco
            cnv1.Background = Brushes.White;
            cnv2.Background = Brushes.White;
            //Lo alinea a la izquierda
            cnv1.HorizontalAlignment = HorizontalAlignment.Left;
            cnv2.HorizontalAlignment = HorizontalAlignment.Left;
            //Le añade margenes
            cnv1.Margin.Equals(5);
            cnv2.Margin.Equals(5);
            cnv1.Margin.Top.Equals(5);
            cnv2.Margin.Top.Equals(5);
            //Actualiza la TextBox de nombre de documento con el nombre que le hayamos dado en la ventana
            Nombre_Tipo_Documento.Text = m_nombre_Etiqueta;
            //Cambia el tamaño de la regla y su valor máximo para que sea igual al del canvas
            ruler1h.Width = cnv1.Width + 6;
            ruler1v.Height = cnv1.Height + 6;
            ruler2h.Width = cnv2.Width + 6;
            ruler2v.Height = cnv2.Height + 6;
            ruler1h.MaxValue = cnv1.Width / 8;
            ruler1v.MaxValue = cnv1.Height / 8;
            ruler2h.MaxValue = cnv2.Width / 8;
            ruler2v.MaxValue = cnv2.Height / 8;
            ////Añade el canvas al StackPanel de diseño
            //Design_Frame1.Children.Add(cnv1);     
            //Design_Frame2.Children.Add(cnv2);            
            //Centra el foco por defecto en el Tab1 "WEIGHT"
            Tbt1.Focus();
            
           

        }

        #endregion
        #region Eventos botones principales //Eventos de los principales botones de creación de objetos
        private void Nueva_Etiqueta_Click(object sender, RoutedEventArgs e)//Abre la Ventana de creación de etiqueta
        {
            Nueva_Etiqueta_Ventana nueva_Etiqueta_Ventana = new Nueva_Etiqueta_Ventana(m_ancho_Etiqueta, m_alto_Etiqueta, m_nombre_Etiqueta);
            nueva_Etiqueta_Ventana.Show();
        }
        private void Abrir_Etiqueta_Click(object sender, RoutedEventArgs e)//Abre el dialogo para abrir un xml
        {
            //File opener
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.FileOk += Dlg_FileOk;
            dlg.FileName = m_nombre_Etiqueta;
            dlg.ShowDialog();
            
        }
        private void Dlg_FileOk(object sender, CancelEventArgs e)
        {

            //Limpia los datos de las etiquetas existentes
            
            cnv1.Children.Clear();
            cnv2.Children.Clear();
            m_lstLabels.Clear();
            lbl_cnt_weight = 0;
            lbl_cnt_resume = 0;

            bool unique_section_creation = false;
            string l_strLine;
            System.IO.StreamReader l_fsXML = new System.IO.StreamReader(dlg.OpenFile());

            int l_iTypeLine = 0, l_iStart = 0, l_iEqual = 0, l_iEnd = 0, l_iEndItem = 0, l_iOpenValue = 0, l_iCloseValue = 0;
            string l_strTypeLine, l_strField, l_strValue;

            string l_strCondition = "", l_strCondResource = "", l_strcondValue = "";
            bool l_bCond = false;
            string l_strName_gen = "";
            string l_strOrientation_gen = "";

            while (!l_fsXML.EndOfStream)
            {
                l_strLine = l_fsXML.ReadLine();

                if (l_strLine == "")

                    continue;
                l_iTypeLine = (l_strLine.IndexOf('<') + 1);
                l_iEndItem = (l_strLine.IndexOf('>'));
                l_iEnd = l_strLine.IndexOf(' ', l_iTypeLine);


                if (l_iEnd == -1)
                    l_iEnd = l_iEndItem;


                l_strTypeLine = l_strLine.Substring(l_iTypeLine, l_iEnd - l_iTypeLine);

                if (l_strTypeLine == "report")
                {
                    int l_iWidth = 0, l_iHeight = 0;
                    string l_strOrientation = "";
                    string l_strName = "";
                    l_strName_gen = "";
                    l_strOrientation_gen = "";

                    while ((l_iEnd < l_iEndItem))
                    {
                        l_iStart = l_iEnd + 1;
                        l_iEqual = l_strLine.IndexOf('=', l_iStart) + 1;
                        l_iOpenValue = l_strLine.IndexOf('\"', l_iEqual) + 1;
                        l_iCloseValue = l_strLine.IndexOf('\"', l_iOpenValue) + 1;
                        l_iEnd = l_strLine.IndexOf(' ', l_iCloseValue);

                        if (l_iEnd == -1)
                            l_iEnd = l_iEndItem;

                        l_strField = l_strLine.Substring(l_iStart, l_iEqual - l_iStart - 1);
                        l_strValue = l_strLine.Substring(l_iOpenValue, l_iCloseValue - l_iOpenValue - 1);

                        if (l_strField == "page_orientation")
                        {
                            l_strOrientation = l_strValue;
                        }
                        else if (l_strField == "page_width")
                        {
                            l_iWidth = Convert.ToInt32(l_strValue);
                            if (l_iWidth > 448)
                                l_iWidth = 448;
                        }
                        else if (l_strField == "page_height")
                        {
                            l_iHeight = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "type") { }
                        else if (l_strField == "name")
                        {
                            l_strName = l_strValue;
                            
                        }
                        else if (l_strField == "page") { }
                        else if (l_strField == "width") { }
                        else if (l_strField == "height") { }
                        else if (l_strField == "left_margin") { }
                        else if (l_strField == "version") { }
                        else if (l_strField == "units") { }
                        else if (l_strField == "auto_offset") { }
                    }

                    if ((l_strOrientation == "portrait") || (l_strOrientation == "Portrait") || (l_strOrientation == "PORTRAIT"))
                        labelOrientation = "portrait";
                    else if ((l_strOrientation == "landscape") || (l_strOrientation == "Landscape") || (l_strOrientation == "LANDSCAPE"))
                        labelOrientation = "landscape";
                    l_strOrientation_gen = l_strOrientation;
                    Nombre_Tipo_Documento.Text = l_strName;
                    Document_Width.Text = Convert.ToString(l_iWidth);
                    Document_Height.Text = Convert.ToString(l_iHeight);

                }
                else if (l_strTypeLine == "if")
                {
                    while ((l_iEnd < l_iEndItem))
                    {
                        l_iStart = l_iEnd + 1;
                        l_iEqual = l_strLine.IndexOf('=', l_iStart) + 1;
                        l_iOpenValue = l_strLine.IndexOf('\"', l_iEqual) + 1;
                        l_iCloseValue = l_strLine.IndexOf('\"', l_iOpenValue) + 1;
                        l_iEnd = l_strLine.IndexOf(' ', l_iCloseValue);

                        if (l_iEnd == -1)
                            l_iEnd = l_iEndItem;

                        l_strField = l_strLine.Substring(l_iStart, l_iEqual - l_iStart - 1);
                        l_strValue = l_strLine.Substring(l_iOpenValue, l_iCloseValue - l_iOpenValue - 1);

                        if (l_strField == "condition")
                        {
                            l_strCondition = l_strValue;
                        }

                        else if (l_strField == "resource1")
                        {
                            l_strCondResource = l_strValue;
                        }
                        else if (l_strField == "value2")
                        {
                            l_strcondValue = l_strValue;
                        }
                    }
                    l_bCond = true;
                }
                else if (l_strTypeLine == "section")
                {
                    int l_iWidth = 0, l_iHeight = 0, l_iX = 0, l_iY = 0;
                    string l_strName = "";

                    while ((l_iEnd < l_iEndItem))
                    {
                        l_iStart = l_iEnd + 1;
                        l_iEqual = l_strLine.IndexOf('=', l_iStart) + 1;
                        l_iOpenValue = l_strLine.IndexOf('\"', l_iEqual) + 1;
                        l_iCloseValue = l_strLine.IndexOf('\"', l_iOpenValue) + 1;
                        l_iEnd = l_strLine.IndexOf(' ', l_iCloseValue);

                        if (l_iEnd == -1)
                            l_iEnd = l_iEndItem;

                        l_strField = l_strLine.Substring(l_iStart, l_iEqual - l_iStart - 1);
                        l_strValue = l_strLine.Substring(l_iOpenValue, l_iCloseValue - l_iOpenValue - 1);

                        if (l_strField == "name")
                        {
                            l_strName = l_strValue;
                            l_strName_gen = l_strValue;
                        }
                        else if (l_strField == "width")
                        {
                            l_iWidth = Convert.ToInt32(l_strValue);
                            if (l_strOrientation_gen == "portrait")
                            {
                                
                                if (l_iWidth > 448)
                                    l_iWidth = 448;
                            }
                            
                        }
                        else if (l_strField == "height")
                        {
                            l_iHeight = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "x")
                        {
                            l_iX = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "y")
                        {
                            l_iY = Convert.ToInt32(l_strValue);
                        }
                    }
                    if (unique_section_creation == false)
                    {
                        Crear_Etiqueta_Reader(dlg.FileName, l_iWidth / 8, l_iHeight / 8, l_strOrientation_gen);
                        unique_section_creation = true;
                    }
                    //l_iWidth, l_iHeight, l_iX, l_iY, l_strName, l_bCond, l_strCondition, l_strCondResource, l_strcondValue
                    l_bCond = false;
                    l_strCondition = "";
                    l_strCondResource = "";
                    l_strcondValue = "";
                }
                else if (l_strTypeLine == "item")
                {
                    int l_iWidth = 0, l_iHeight = 0, l_iX = 0, l_iY = 0, l_iId = -1, l_iLineWidth = 3;
                    string l_strType = "", l_strResource = "", l_strAlignment = "", l_strBarcode = "", l_strOrientation = "", l_strFont = "";

                    while ((l_iEnd < l_iEndItem))
                    {
                        l_iStart = l_iEnd + 1;
                        l_iEqual = l_strLine.IndexOf('=', l_iStart) + 1;
                        l_iOpenValue = l_strLine.IndexOf('\"', l_iEqual) + 1;
                        l_iCloseValue = l_strLine.IndexOf('\"', l_iOpenValue) + 1;
                        l_iEnd = l_strLine.IndexOf(' ', l_iCloseValue);

                        if (l_iEnd == -1)
                            l_iEnd = l_iEndItem;

                        l_strField = l_strLine.Substring(l_iStart, l_iEqual - l_iStart - 1);
                        l_strValue = l_strLine.Substring(l_iOpenValue, l_iCloseValue - l_iOpenValue - 1);

                        if (l_strField == "resource")
                        {
                            if (l_strValue == "line.Operations")
                                l_strResource = "total.Operations";
                            else if (l_strValue == "line.netAmount")
                                l_strResource = "total.Amount";
                            else
                                l_strResource = l_strValue;
                        }
                        else if (l_strField == "id")
                        {
                            l_iId = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "type")
                        {
                            l_strType = l_strValue;
                        }
                        else if (l_strField == "alignment")
                        {
                            l_strAlignment = l_strValue;
                        }
                        else if (l_strField == "font")
                        {
                            l_strFont = l_strValue;
                        }
                        else if (l_strField == "barcode_type")
                        {
                            l_strBarcode = l_strValue;
                        }
                        else if (l_strField == "orientation")
                        {
                            l_strOrientation = l_strValue;
                        }
                        else if (l_strField == "line_width")
                        {
                            l_iLineWidth = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "width")
                        {
                            l_iWidth = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "height")
                        {
                            l_iHeight = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "x")
                        {
                            l_iX = Convert.ToInt32(l_strValue);
                        }
                        else if (l_strField == "y")
                        {
                            l_iY = Convert.ToInt32(l_strValue);
                        }
                    }

                    if (l_iWidth <= 0)
                    {
                        if (l_strType == "logo")
                            l_iWidth = 448;
                        else if (l_strType == "seal")
                            l_iWidth = 64;
                    }
                    if (l_iHeight <= 0)
                    {
                        if (l_strType == "logo")
                            l_iHeight = 100;
                        else if (l_strType == "seal")
                            l_iWidth = 64;
                    }

                    if (l_strAlignment == "center")
                    {
                        l_iX = l_iX - l_iWidth / 2;
                    }
                    else if (l_strAlignment == "right")
                    {
                        l_iX = l_iX - l_iWidth;
                    }
                    
                    if (l_strType == "label")
                    {
                        string l_strText = "";
                        l_iStart = l_strLine.IndexOf('>') + 1;
                        l_iEnd = l_strLine.IndexOf('<', l_iStart);

                        l_strText = l_strLine.Substring(l_iStart, l_iEnd - l_iStart);
                        Add_Texto_Reader(l_strText, l_strAlignment, l_strFont, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                    else if (l_strType == "field")
                    {
                        Add_Campo_Reader(l_strResource, l_iId, l_strAlignment, l_strFont, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                    else if (l_strType == "shape")
                    {
                       Add_Cajeado_Reader(l_iId, l_strAlignment, l_iLineWidth, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                    else if (l_strType == "logo")
                    {
                        Add_Imagen_Reader(l_strResource, l_strAlignment, l_iId, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                    else if (l_strType == "seal")
                    {
                        Add_Sello_Reader(l_strResource, l_strAlignment, l_iId, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                    else if (l_strType == "barcode")
                    {
                        Add_Barcode_Reader(l_strResource, l_strAlignment, l_iId, l_iWidth, l_iHeight, l_iX, l_iY, l_bCond, l_strCondition, l_strCondResource, l_strcondValue, l_strName_gen);
                    }
                }
                else if (l_strTypeLine == "/if")
                {
                    l_strCondition = "";
                    l_strCondResource = "";
                    l_strcondValue = "";
                    l_bCond = false;
                }
            }
            l_fsXML.Close();
        }
        private void Guardar_Etiqueta_Click(object sender, RoutedEventArgs e) //Pone filtros para guardar xml y muestra el dialogo
        {
            dlg_save.DefaultExt = ".xml";
            dlg_save.Filter = "XML Files (*.xml)|*.xml";
            dlg_save.FileOk += Dlg_save_FileOk;
            dlg_save.FileName = m_nombre_Etiqueta;
            dlg_save.ShowDialog();        
        }
        private void Dlg_save_FileOk(object sender, System.ComponentModel.CancelEventArgs e) //Escribe linea por linea en el
                                                                                            //xml lo que tenemos en los canvas
        {
            //Crea el archivo sobre el que escribimos
            StreamWriter l_fsXML = new StreamWriter(dlg_save.OpenFile());
            
            m_lstLabels.Sort((x, y) => x.type.CompareTo(y.type));
            string l_strAuxOrientation = "";
            //switch (labelOrientation) //Switch para elegir orientación de documento
            //{
            //    case "vertical":
            //        l_strAuxOrientation = "portrait";
            //        break;
            //    case "horizontal":
            //        l_strAuxOrientation = "landscape";
            //        break;
            //}
            //Comienzo a escribir cada linea del xml
            l_fsXML.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            l_fsXML.WriteLine("<report page_orientation=\"" + labelOrientation + "\" page_width=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Width / 8)) + "\" page_height=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Height / 8)) + "\" type=\"label\" name=\"" + m_nombre_Etiqueta + "\" page=\"Custom\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" left_margin=\"0\" version=\"1\" units=\"pixels\" auto_offset=\"1\">");
            if (lbl_cnt_weight > 0)
            {
                l_fsXML.WriteLine("\t<if condition=\"equal\" resource1=\"header.LabelType\" value2=\"WEIGHT\">");
                l_fsXML.WriteLine("\t\t<section name=\"WEIGHT\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" x=\"" + Convert.ToString(Decimal.Truncate(0)) + "\" y=\"" + Convert.ToString(Decimal.Truncate(0)) + "\">");
                for (int idx = 0; idx < m_lstLabels.Count(); idx++) //Escribe en cada linea la información de cada objeto añadido a weight
                {
                    int PosX = (int)Canvas.GetLeft(m_lstLabels[idx].widget);
                    int PosY = (int)Canvas.GetTop(m_lstLabels[idx].widget);
                    string bold = "";
                    if (m_lstLabels[idx].negrita)
                    {
                        bold = "bold";
                    }
                    if (m_lstLabels[idx].labelSelec == "weight")
                    {
                        if (m_lstLabels[idx].type == types_items.lbl)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width/2);
                            }

                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");
                            
                                l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align +"\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\""+ m_lstLabels[idx].font+"  " + bold + " "+ m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Children.OfType<TextBlock>().First().Text + "</item>");
                            
                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + " " + bold + " " + m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Children.OfType<TextBlock>().First().Text + "</item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.box)
                        {
                            if(m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");
                                l_fsXML.WriteLine("\t\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");

                            }
                            l_fsXML.WriteLine("\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");
                        }
                        if (m_lstLabels[idx].type == types_items.barcode)
                        {
                            string barcode_type_H = "";
                            switch (m_lstLabels[idx].barcode_type)
                            {
                                case "ean8":
                                    barcode_type_H = "header.EAN8";
                                    break;
                                case "ean13":
                                    barcode_type_H = "header.EAN13";
                                    break;
                                case "ean14":
                                    barcode_type_H = "header.EAN14";
                                    break;
                                case "code39":
                                    barcode_type_H = "header.CODE39";//Quizas no sea este, consultar con carlos
                                    break;
                                case "code128":
                                    barcode_type_H = "header.CODE128";
                                    break;

                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"barcode\" id=\"-1\" resource=\"" + barcode_type_H + "\" barcode_type=\"" + m_lstLabels[idx].barcode_type + "\" alignment=\"\" orientation =\"\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"barcode\" id=\"-1\" resource=\"" + barcode_type_H + "\" barcode_type=\"" + m_lstLabels[idx].barcode_type + "\" alignment=\"\" orientation =\"\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.field)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t\t<item type=\"field\" resource=\"" + m_lstLabels[idx].textbox_type + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + "  " + bold + " " + m_lstLabels[idx].font_size + "\" id=\"" + m_lstLabels[idx].id + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"field\" resource=\"" + m_lstLabels[idx].textbox_type +"\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + " " + bold + " " + m_lstLabels[idx].font_size + "\" id=\"" + m_lstLabels[idx].id + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.logo)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }

                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"logo\" id=\"-1\" resource=\"header.Logo\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"logo\" id=\"-1\" resource=\"header.Logo\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.seal)
                        {

                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"seal\" id=\"" + m_lstLabels[idx].id + "\" resource=\"header.Seal\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"seal\" id=\"" + m_lstLabels[idx].id + "\" resource=\"header.Seal\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }


                    }
                }
                l_fsXML.WriteLine("</section>");
                l_fsXML.WriteLine("</if>");
            }
            if (lbl_cnt_resume > 0)
            {
                l_fsXML.WriteLine("\t<if condition=\"equal\" resource1=\"header.LabelType\" value2=\"RESUME\">");
                l_fsXML.WriteLine("\t\t<section name=\"RESUME\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv2.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv2.Height))) + "\" x=\"" + Convert.ToString(Decimal.Truncate(0)) + "\" y=\"" + Convert.ToString(Decimal.Truncate(0)) + "\">");
                for (int idx = 0; idx < m_lstLabels.Count(); idx++)
                {
                    int PosX = (int)Canvas.GetLeft(m_lstLabels[idx].widget);
                    int PosY = (int)Canvas.GetTop(m_lstLabels[idx].widget);
                    string bold = "";                    
                    if (m_lstLabels[idx].negrita)
                    {
                        bold = "bold";
                    }
                    if (m_lstLabels[idx].labelSelec == "resume")
                    {
                        if (m_lstLabels[idx].type == types_items.lbl)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }

                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + "  " + bold + " " + m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Children.OfType<TextBlock>().First().Text + "</item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + " " + bold + " " + m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Children.OfType<TextBlock>().First().Text + "</item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.box)
                        {
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");
                                l_fsXML.WriteLine("\t\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");

                            }
                            l_fsXML.WriteLine("\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");
                        }
                        if (m_lstLabels[idx].type == types_items.barcode)
                        {
                            string barcode_type_H = "";
                            switch (m_lstLabels[idx].barcode_type)
                            {
                                case "ean8":
                                    barcode_type_H = "header.EAN8";
                                    break;
                                case "ean13":
                                    barcode_type_H = "header.EAN13";
                                    break;
                                case "ean14":
                                    barcode_type_H = "header.EAN14";
                                    break;
                                case "code39":
                                    barcode_type_H = "header.CODE39";//Quizas no sea este, consultar con carlos
                                    break;
                                case "code128":
                                    barcode_type_H = "header.CODE128";
                                    break;

                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"barcode\" id=\"-1\" resource=\"" + barcode_type_H + "\" barcode_type=\"" + m_lstLabels[idx].barcode_type + "\" alignment=\"\" orientation =\"\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"barcode\" id=\"-1\" resource=\"" + barcode_type_H + "\" barcode_type=\"" + m_lstLabels[idx].barcode_type + "\" alignment=\"\" orientation =\"\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.field)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t\t<item type=\"field\" resource=\"" + m_lstLabels[idx].textbox_type + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + "  " + bold + " " + m_lstLabels[idx].font_size + "\" id=\"" + m_lstLabels[idx].id + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"field\" resource=\"" + m_lstLabels[idx].textbox_type + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"" + m_lstLabels[idx].align + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"" + m_lstLabels[idx].font + " " + bold + " " + m_lstLabels[idx].font_size + "\" id=\"" + m_lstLabels[idx].id + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.logo)
                        {
                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }

                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"logo\" id=\"-1\" resource=\"header.Logo\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"logo\" id=\"-1\" resource=\"header.Logo\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }
                        if (m_lstLabels[idx].type == types_items.seal)
                        {

                            if (m_lstLabels[idx].align == "right")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width);
                            }
                            if (m_lstLabels[idx].align == "center")
                            {
                                PosX = (int)(PosX + m_lstLabels[idx].widget.Width / 2);
                            }
                            if (m_lstLabels[idx].condition != "none")
                            {
                                l_fsXML.WriteLine("\t\t\t<if condition=\"" + m_lstLabels[idx].condition + "\" resource1=\"" + m_lstLabels[idx].cond_resource + "\" value2=\"" + m_lstLabels[idx].cond_value + "\">");

                                l_fsXML.WriteLine("\t\t\t<item type=\"seal\" id=\"" + m_lstLabels[idx].id + "\" resource=\"header.Seal\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                                l_fsXML.WriteLine("\t\t\t</if>");
                            }
                            else
                            {
                                l_fsXML.WriteLine("\t\t\t<item type=\"seal\" id=\"" + m_lstLabels[idx].id + "\" resource=\"header.Seal\" alignment=\"" + m_lstLabels[idx].align + "\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\"></item>");

                            }
                        }

                    }
                }
                l_fsXML.WriteLine("</section>");
                l_fsXML.WriteLine("</if>");
            }                 
            l_fsXML.Close();
        }
        private void Add_Texto_Click(object sender, RoutedEventArgs args)//Añade un objeto tipo texto
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if (Tbt1.IsSelected || Tbt2.IsSelected)//Solo actúa si uno de los dos 'tabs' está seleccionado
            {
                
                label_item l_lblitAux = new label_item();//Crea la struct del objeto
                l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
                TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
                Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
                //Propiedades de los elementos
                //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
                //usando o no las propiedades que necesitamos
                tb.Margin = new Thickness(5, 2, 2, 2);                
                l_lblitAux.widget.MinHeight = 10;
                l_lblitAux.widget.MinWidth = 10;
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 1;
                rect.StrokeDashArray = new DoubleCollection(new double[] { 4, 4 });
                
                rect.SnapsToDevicePixels = true;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 60;
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 25;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.lbl;
                l_lblitAux.id = -1;//Cuidado con el tema de los ids                
                l_lblitAux.is_selected = false;
                l_lblitAux.font = "Arial";
                select_font_object(l_lblitAux.font);
                l_lblitAux.font_size = 14;
                size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.is_selected = true;
                l_lblitAux.widget.Focus();
                l_lblitAux.align = "left";
                select_alignment_object(l_lblitAux.align);
                //Actualiza los datos de condición del TextBox
                l_lblitAux.condition = "none";
                l_lblitAux.cond_resource = "line.SaleForm";
                cond_line_saleform.IsSelected = true;
                //Se crea el grupo de eventos a los que reaccionará
                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;
                
                //Actualiza el textbox de ancho objeto con el valor puesto por defecto
                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);
                
                //Se posiciona el elemento en el canvas
                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);
                //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
                if (Tbt1.IsSelected)
                {
                    l_lblitAux.weight_num = lbl_cnt_weight;
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "lbl_weight_" + lbl_cnt_weight;                    
                    tb.Text = "Text " + lbl_cnt_weight;
                    Text_tb.Text = tb.Text;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    l_lblitAux.resume_num = lbl_cnt_resume;
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "lbl_resume_" + lbl_cnt_resume;
                    tb.Text = "Text " + lbl_cnt_resume;
                    Text_tb.Text = tb.Text;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
                //Se le asigna un número a su variable que controla que número de elemento en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                sel_prop_text.IsEnabled = true;
                sel_prop_barcode.IsEnabled = false;
                sel_prop_id.IsEnabled = false;
                sel_prop_campo.IsEnabled = false;
                alignment_toolbar.IsEnabled = true;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = false;

            }            
        } 
        private void Add_Campo_Click(object sender, RoutedEventArgs e)//Añade un objeto tipo Campo
        {
            for (int i = 0; i < m_lstLabels.Count(); i++) //Deselecciona los objetos seleccionados
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if (Tbt1.IsSelected || Tbt2.IsSelected)
            {
                label_item l_lblitAux = new label_item();//Crea la struct del objeto
                l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
                TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
                Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
                                                 //Propiedades de los elementos
                                                 //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
                                                 //usando o no las propiedades que necesitamos
                tb.Margin = new Thickness(5, 2, 2, 2);
                l_lblitAux.widget.MinHeight = 10;
                l_lblitAux.widget.MinWidth = 10;
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 1;
                rect.StrokeDashArray = new DoubleCollection(new double[] { 4, 4 });
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 80;
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 35;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.field;
                l_lblitAux.id = 1;//Cuidado con el tema de los ids
                id_update_toolbar(l_lblitAux.id);
                l_lblitAux.is_selected = false;
                l_lblitAux.font = "Arial";
                select_font_object(l_lblitAux.font);
                l_lblitAux.font_size = 14;
                size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.is_selected = true;
                l_lblitAux.widget.Focus();
                l_lblitAux.condition = "none";
                select_condition_object(l_lblitAux.condition);
                l_lblitAux.align = "left";
                select_alignment_object(l_lblitAux.align);
                l_lblitAux.textbox_type = "line.SaleForm";
                tb.Text = l_lblitAux.textbox_type;
                select_type_lbl(l_lblitAux.textbox_type);
                //Actualiza los datos de condición del TextBox
                l_lblitAux.condition = "none";
                l_lblitAux.cond_resource = "line.SaleForm";
                cond_line_saleform.IsSelected = true;
                //Se crea el grupo de eventos a los que reaccionará
                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

                //Actualiza el textbox de ancho objeto con el valor puesto por defecto
                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

                //Se posiciona el elemento en el canvas
                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);
                //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
                if (Tbt1.IsSelected)
                {
                    l_lblitAux.weight_num = lbl_cnt_weight;
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "field_weight_" + lbl_cnt_weight;
                    Text_tb.Text = tb.Text;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    l_lblitAux.resume_num = lbl_cnt_resume;
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "field_resume_" + lbl_cnt_resume;
                    Text_tb.Text = tb.Text;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
                                                   //Se le asigna un número a su variable que controla que número de elemento en
                                                   //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                sel_prop_text.IsEnabled = false;
                sel_prop_barcode.IsEnabled = false;
                sel_prop_id.IsEnabled = true;
                sel_prop_campo.IsEnabled = true;
                alignment_toolbar.IsEnabled = true;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = false;
            }
                       
        }
        private void Add_Cajeado_Click(object sender, RoutedEventArgs e)//Añade un cuadrado vacío pintando su borde
        {
            for (int i = 0; i < m_lstLabels.Count(); i++) //Deselecciona los objetos seleccionados
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if ( Tbt1.IsSelected || Tbt2.IsSelected )
            {
                label_item l_lblitAux = new label_item();

                l_lblitAux.widget = new Grid();
                TextBlock tb = new TextBlock();
                Rectangle rect = new Rectangle();

                tb.Margin = new Thickness(5, 2, 2, 2);

                l_lblitAux.widget.MinHeight = 10;
                l_lblitAux.widget.MinWidth = 10;

                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 1;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 80;
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 80;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";
                l_lblitAux.type = types_items.box;
               
                tb.TextWrapping = TextWrapping.Wrap;
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.is_selected = true;
                l_lblitAux.condition = "none";
                select_condition_object(l_lblitAux.condition);
                select_border_thin(1);

                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);

                if (Tbt1.IsSelected)
                {
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "box_weight_" + lbl_cnt_weight;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "box_resume_" + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);
                l_lblitAux.widget.Children.Add(tb);

                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);

                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo           
                sel_prop_text.IsEnabled = false;
                sel_prop_barcode.IsEnabled = false;
                sel_prop_id.IsEnabled = false;
                sel_prop_campo.IsEnabled = false;
                alignment_toolbar.IsEnabled = false;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = true;
            }
        }
        private void Add_Imagen_Click(object sender, RoutedEventArgs e)//Añade el logo
        {
            for (int i = 0; i < m_lstLabels.Count(); i++) //Deselecciona los objetos seleccionados
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if (Tbt1.IsSelected || Tbt2.IsSelected)
            {
                ImageBrush myBrush = new ImageBrush();                
                myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/logo_background.png"));
                label_item l_lblitAux = new label_item();//Crea la struct del objeto
                l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
                TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
                Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
                //Propiedades de los elementos
                //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
                //usando o no las propiedades que necesitamos
                tb.Margin = new Thickness(5, 2, 2, 2);
                l_lblitAux.widget.MinHeight = 10;
                l_lblitAux.widget.MinWidth = 10;
                rect.Fill = myBrush;
                rect.StrokeThickness = 1;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 400;
                //new BitmapImage(ResourceAccessor.Get("Images/1.png"))
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.condition = "none";
                select_condition_object(l_lblitAux.condition);
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 120;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.align = "center";
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.logo;//Tipo de elemento ||||ES IMPORTANTE PARA EL FUNCIONAMIENTO
                l_lblitAux.id = -1;//Cuidado con el tema de los ids
                l_lblitAux.is_selected = true;
                l_lblitAux.widget.Focus();
                //Se crea el grupo de eventos a los que reaccionará
                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;
                

                //Actualiza el textbox de ancho objeto con el valor puesto por defecto
                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);
                //Se posiciona el elemento en el canvas
                Canvas.SetLeft(l_lblitAux.widget, cnv1.Width/2 - l_lblitAux.widget.Width/2);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);
                //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
                if (Tbt1.IsSelected)
                {
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "logo_weight_" + lbl_cnt_weight;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "logo_resume_" + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock
                //Se le asigna un número a su variable que controla que número de elemento es en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                sel_prop_text.IsEnabled = false;
                sel_prop_barcode.IsEnabled = false;
                sel_prop_id.IsEnabled = false;
                alignment_toolbar.IsEnabled = false;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = false;
                sel_prop_campo.IsEnabled = false;

            }
        }
        private void Add_Sello_Click(object sender, RoutedEventArgs e)//Añade un sello
        {
            for (int i = 0; i < m_lstLabels.Count(); i++) //Deselecciona los objetos seleccionados
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if (Tbt1.IsSelected || Tbt2.IsSelected)
            {
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/seal_background.png"));
                label_item l_lblitAux = new label_item();//Crea la struct del objeto
                l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
                TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
                Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
                //Propiedades de los elementos
                //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
                //usando o no las propiedades que necesitamos
                tb.Margin = new Thickness(5, 2, 2, 2);
                l_lblitAux.widget.MinHeight = 10;
                l_lblitAux.widget.MinWidth = 10;
                rect.Fill = myBrush;
                rect.StrokeThickness = 1;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 50;
                //new BitmapImage(ResourceAccessor.Get("Images/1.png"))
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.condition = "none";
                select_condition_object(l_lblitAux.condition);
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 50;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.align = "center";
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.seal;//Tipo de elemento ||||ES IMPORTANTE PARA EL FUNCIONAMIENTO
                seal_counter++;
                l_lblitAux.id = seal_counter;//Cuidado con el tema de los ids
                id_update_toolbar(l_lblitAux.id);
                l_lblitAux.is_selected = true;
                l_lblitAux.widget.Focus();
                //Se crea el grupo de eventos a los que reaccionará
                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;


                //Actualiza el textbox de ancho objeto con el valor puesto por defecto
                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);
                //Se posiciona el elemento en el canvas
                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);
                //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
                if (Tbt1.IsSelected)
                {
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "seal_weight_" + lbl_cnt_weight;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "seal_resume_" + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock
                //Se le asigna un número a su variable que controla que número de elemento es en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                sel_prop_text.IsEnabled = false;
                sel_prop_barcode.IsEnabled = false;
                sel_prop_id.IsEnabled = true;
                alignment_toolbar.IsEnabled = false;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = false;
                sel_prop_campo.IsEnabled = false;

            }
        }
        private void Add_Barcode_Click(object sender, RoutedEventArgs e)//Añade un código de barras
        {
            for (int i = 0; i < m_lstLabels.Count(); i++) //Deselecciona los objetos seleccionados
            {
                label_item lbl = m_lstLabels[i];
                lbl.is_selected = false;
                lbl.widget.Background = Brushes.Transparent;
                lbl.widget.Opacity = 1;
                m_lstLabels[i] = lbl;
            }
            if (Tbt1.IsSelected || Tbt2.IsSelected)
            {
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/barcode_background.png"));
                label_item l_lblitAux = new label_item();//Crea la struct del objeto
                l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
                TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
                Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
                //Propiedades de los elementos
                //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
                //usando o no las propiedades que necesitamos
                tb.Margin = new Thickness(5, 2, 2, 2);
                l_lblitAux.widget.MinHeight = 99;
                l_lblitAux.widget.MaxHeight = 99;
                l_lblitAux.widget.MinWidth = 209;
                l_lblitAux.widget.MaxWidth = 209;
                rect.Fill = myBrush;
                rect.StrokeThickness = 1;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 209;
                l_lblitAux.widget.Background = Brushes.LightGreen;
                l_lblitAux.widget.Opacity = 0.5;
                l_lblitAux.condition = "none";
                select_condition_object(l_lblitAux.condition);
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 99;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.barcode;//Tipo de elemento ||||ES IMPORTANTE PARA EL FUNCIONAMIENTO
                l_lblitAux.id = -1;//Cuidado con el tema de los ids
                l_lblitAux.is_selected = true;
                l_lblitAux.widget.Focus();
                l_lblitAux.barcode_type = "ean13";
                select_barcode_type(l_lblitAux.barcode_type);
                //Se crea el grupo de eventos a los que reaccionará
                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;
                
                //Actualiza el textbox de ancho objeto con el valor puesto por defecto
                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);
                //Se posiciona el elemento en el canvas
                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);
                Canvas.SetZIndex(l_lblitAux.widget, 0);
                //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
                if (Tbt1.IsSelected)
                {
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "barcode_weight_" + lbl_cnt_weight;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "barcode_resume_" + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock
                //Se le asigna un número a su variable que controla que número de elemento es en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                lbl_single_select = l_lblitAux.idCount;
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                sel_prop_text.IsEnabled = false;
                sel_prop_barcode.IsEnabled = true;
                sel_prop_id.IsEnabled = false;
                alignment_toolbar.IsEnabled = false;
                condition_toolbar.IsEnabled = true;
                border_thin_selector.IsEnabled = false;
                sel_prop_campo.IsEnabled = false;

            }
        }
        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            Nueva_Etiqueta_Ventana nueva_Etiqueta_Ventana = new Nueva_Etiqueta_Ventana(m_ancho_Etiqueta, m_alto_Etiqueta, m_nombre_Etiqueta);
            nueva_Etiqueta_Ventana.Show();
        }
        private void Abrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFile_Click();
        }
        private void Guardar_Click(object sender, RoutedEventArgs e)
        {

            dlg_save.DefaultExt = ".xml";
            dlg_save.Filter = "XML Files (*.xml)|*.xml";
            dlg_save.FileOk += Dlg_save_FileOk;
            dlg_save.FileName = m_nombre_Etiqueta;
            dlg_save.ShowDialog();
        }
        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
        #region Funciones para pintar las etiquetas leídas
        private void Add_Texto_Reader(string l_strText, string l_strAlignment, string l_strFont, double l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            string[] font_array = l_strFont.Split(' ');
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = Brushes.Transparent;
            rect.StrokeThickness = 1;
            rect.StrokeDashArray = new DoubleCollection(new double[] { 4, 4 });

            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.lbl;
            l_lblitAux.id = -1;//Cuidado con el tema de los ids                
            l_lblitAux.is_selected = false;
            l_lblitAux.font = font_array[0];
            select_font_object(l_lblitAux.font);
            l_lblitAux.font_size = Convert.ToInt32(font_array.Last());
            size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            select_alignment_object(l_lblitAux.align);
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, l_iX);
            Canvas.SetTop(l_lblitAux.widget, l_iY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "lbl_weight_" + lbl_cnt_weight;
                tb.Text = l_strText;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "lbl_resume_" + lbl_cnt_resume;
                tb.Text = l_strText;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);


        }
        private void Add_Campo_Reader(string l_strResource, int l_iId, string l_strAlignment, string l_strFont, int l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            string[] font_array = l_strFont.Split(' ');
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = Brushes.Transparent;
            rect.StrokeThickness = 1;
            rect.StrokeDashArray = new DoubleCollection(new double[] { 4, 4 });

            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.field;
            l_lblitAux.id = l_iId;//Cuidado con el tema de los ids  
            l_lblitAux.font = font_array[0];
            l_lblitAux.font_size = Convert.ToInt32(font_array.Last());
            if(l_strAlignment == "left")
            {
                tb.TextAlignment = TextAlignment.Left;
            }
            else if (l_strAlignment == "right")
            {
                tb.TextAlignment = TextAlignment.Right;
            }
            else if (l_strAlignment == "center")
            {
                tb.TextAlignment = TextAlignment.Center;
            }
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            l_lblitAux.textbox_type = l_strResource;
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);
            int PosX = l_iX;
            int PosY = l_iY;
            
            if (l_strAlignment == "right")
            {
                PosX = l_iX;
            }
            if (l_strAlignment == "center")
            {
                PosX = l_iX;
            }
            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, PosX);
            Canvas.SetTop(l_lblitAux.widget, PosY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "field_weight_" + lbl_cnt_weight;
                tb.Text = l_strResource;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "field_resume_" + lbl_cnt_resume;
                tb.Text = tb.Text = l_strResource;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);

        }
        private void Add_Cajeado_Reader(int l_iId, string l_strAlignment, int l_iLineWidth, int l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = Brushes.Transparent;
            rect.StrokeThickness = l_iLineWidth;

            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.box;
            l_lblitAux.id = l_iId;//Cuidado con el tema de los ids 
            size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            l_lblitAux.textbox_type = "box";
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, l_iX);
            Canvas.SetTop(l_lblitAux.widget, l_iY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "box_weight_" + lbl_cnt_weight;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "box_resume_" + lbl_cnt_resume;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);

        }
        private void Add_Imagen_Reader(string l_strResource, string l_strAlignment, int l_iId, int l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/logo_background.png"));
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = myBrush;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.logo;
            l_lblitAux.id = l_iId;//Cuidado con el tema de los ids 
            size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            l_lblitAux.textbox_type = "box";
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

            int PosX = l_iX;
            int PosY = l_iY;
            if (l_strAlignment == "right")
            {
                PosX = l_iX + l_iWidth/2;
            }
            if (l_strAlignment == "center")
            {
                PosX = l_iX;
            }
            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, PosX);
            Canvas.SetTop(l_lblitAux.widget, PosY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "box_weight_" + lbl_cnt_weight;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "box_resume_" + lbl_cnt_resume;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);

        }
        private void Add_Sello_Reader(string l_strResource, string l_strAlignment, int l_iId, int l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/seal_background.png"));
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = myBrush;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.seal;
            l_lblitAux.id = l_iId;//Cuidado con el tema de los ids 
            size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

            int PosX = l_iX;
            int PosY = l_iY;
            if (l_strAlignment == "right")
            {
                PosX = l_iX + l_iWidth / 2;
            }
            if (l_strAlignment == "center")
            {
                PosX = l_iX;
            }
            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, PosX);
            Canvas.SetTop(l_lblitAux.widget, PosY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "box_weight_" + lbl_cnt_weight;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "box_resume_" + lbl_cnt_resume;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);

        }
        private void Add_Barcode_Reader(string l_strResource, string l_strAlignment, int l_iId, int l_iWidth, int l_iHeight, int l_iX, int l_iY, bool l_bCond, string l_strCondition, string l_strCondResource, string l_strcondValue, string l_strName)
        {
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(ResourceAccessor.Get("Resource/barcode_background.png"));
            label_item l_lblitAux = new label_item();//Crea la struct del objeto
            l_lblitAux.widget = new Grid();//El widget principal es de tipo grid
            TextBlock tb = new TextBlock();//Se crea el subtipo textblock para mostrarlo dentro del grid
            Rectangle rect = new Rectangle();//Se crea un rectángulo para dibujar el borde
            //Propiedades de los elementos
            //Estas propiedades van cambiando dependiendo del tipo de elemento que hemos añadido,
            //usando o no las propiedades que necesitamos
            tb.Margin = new Thickness(5, 2, 2, 2);
            l_lblitAux.widget.MinHeight = 10;
            l_lblitAux.widget.MinWidth = 10;
            rect.Fill = myBrush;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            rect.Stroke = Brushes.Black;
            l_lblitAux.widget.Width = l_iWidth;
            tb.Width = l_lblitAux.widget.Width;
            l_lblitAux.widget.Height = l_iHeight;
            tb.Height = l_lblitAux.widget.Height;
            l_lblitAux.orientation = "horizontal";//Horizontal por defecto
            l_lblitAux.type = types_items.barcode;
            l_lblitAux.id = l_iId;//Cuidado con el tema de los ids 
            size_font_object.Text = Convert.ToString(l_lblitAux.font_size);
            l_lblitAux.widget.Background = Brushes.Transparent;
            l_lblitAux.widget.Focus();
            l_lblitAux.align = l_strAlignment;
            //Actualiza los datos de condición del TextBox
            l_lblitAux.condition = l_strCondition;
            l_lblitAux.cond_resource = l_strCondResource;
            l_lblitAux.cond_value = l_strcondValue;
            //Se crea el grupo de eventos a los que reaccionará
            l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            l_lblitAux.widget.MouseMove += Widget_MouseMove;
            l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
            l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;

            //Actualiza el textbox de ancho objeto con el valor puesto por defecto
            Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
            Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

            int PosX = l_iX;
            int PosY = l_iY;
            if (l_strAlignment == "right")
            {
                PosX = l_iX + l_iWidth / 2;
            }
            if (l_strAlignment == "center")
            {
                PosX = l_iX;
            }
            //Se posiciona el elemento en el canvas
            Canvas.SetLeft(l_lblitAux.widget, PosX);
            Canvas.SetTop(l_lblitAux.widget, PosY);
            Canvas.SetZIndex(l_lblitAux.widget, 0);
            //Se añade el elemento al canvas dependiendo del 'Tab' seleccionado
            if (l_strName == "weight" || l_strName == "Weight" || l_strName == "WEIGHT")
            {
                l_lblitAux.weight_num = lbl_cnt_weight;
                lbl_cnt_weight++;
                l_lblitAux.labelSelec = "weight";
                l_lblitAux.widget.Name = "box_weight_" + lbl_cnt_weight;
                Text_tb.Text = tb.Text;
                cnv1.Children.Add(l_lblitAux.widget);
            }
            else if (l_strName == "resume" || l_strName == "Resume" || l_strName == "RESUME")
            {
                l_lblitAux.resume_num = lbl_cnt_resume;
                lbl_cnt_resume++;
                l_lblitAux.labelSelec = "resume";
                l_lblitAux.widget.Name = "box_resume_" + lbl_cnt_resume;
                Text_tb.Text = tb.Text;
                cnv2.Children.Add(l_lblitAux.widget);
            }
            l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
            l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock                
            //Se le asigna un número a su variable que controla que número de elemento en
            //la aplicacion en general
            l_lblitAux.idCount = m_lstLabels.Count();
            lbl_single_select = l_lblitAux.idCount;
            m_lstLabels.Add(l_lblitAux);

        }
        #endregion
        #region Eventos de Control de Objetos en Canvas //Eventos que ocurren dentro del canvas
        private void Widget_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            size = false;
            var sized = sender as Grid;
            sized.ReleaseMouseCapture();
        }
        private void Widget_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            size = true;
            var sized = sender as Grid;
            sized.CaptureMouse();           


        }
        private void Widget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // stop dragging
            drag = false;
            var dragged = sender as Grid;
            dragged.ReleaseMouseCapture();
        }
        private void Widget_MouseMove(object sender, MouseEventArgs e)
        {
            // if dragging, then adjust rectangle position based on mouse movement
            if (drag)
            {
                if (Tbt1.IsSelected)
                {
                    var draggedRectangle = sender as Grid;
                    var mousePos = e.GetPosition(cnv1);
                    //Point newPoint = Mouse.GetPosition(cnv1);
                    double left = mousePos.X - (draggedRectangle.ActualWidth / 2);
                    
                    double top = mousePos.Y - (draggedRectangle.ActualHeight / 2);
                    Canvas.SetLeft(draggedRectangle, left );
                    Canvas.SetTop(draggedRectangle, top );
                    int leftInt = Convert.ToInt32(left);
                    int topInt = Convert.ToInt32(top);
                   
                    Object_X.Text = Convert.ToString(leftInt);
                    Object_Y.Text = Convert.ToString(topInt);
                }
                else if (Tbt2.IsSelected)
                {
                    var draggedRectangle = sender as Grid;
                    var mousePos = e.GetPosition(cnv2);
                    
                    double left = mousePos.X - (draggedRectangle.ActualWidth / 2);
                    double top = mousePos.Y - (draggedRectangle.ActualHeight / 2);
                    Canvas.SetLeft(draggedRectangle, left);
                    Canvas.SetTop(draggedRectangle, top);
                    int leftInt = Convert.ToInt32(left);
                    int topInt = Convert.ToInt32(top);
                    Object_X.Text = Convert.ToString(leftInt);
                    Object_Y.Text = Convert.ToString(topInt);
                }  
            }
            if (size)
            {
                if (Tbt1.IsSelected)
                {
                    
                
                    //resize rectangle
                    var draggedRectangle = sender as Grid;
                    var mousePos = e.GetPosition(cnv1);
                    double width = mousePos.X - Canvas.GetLeft(draggedRectangle);
                    double height = mousePos.Y - Canvas.GetTop(draggedRectangle);
                    

                    if (height > 0 && width > 0)
                    {
                        draggedRectangle.Width = width;
                        draggedRectangle.Height = height;
                        int widthInt = Convert.ToInt32(width);
                        int heightInt = Convert.ToInt32(height);
                        Ancho_Objeto.Text = Convert.ToString(widthInt / 8);
                        Alto_Objeto.Text = Convert.ToString(heightInt / 8);
                        draggedRectangle.Children.OfType<TextBlock>().First().Width = width;
                        draggedRectangle.Children.OfType<TextBlock>().Last().Height = height;
                    }   
                }
                else if (Tbt2.IsSelected)
                {
                    //resize rectangle
                    var draggedRectangle = sender as Grid;
                    var mousePos = e.GetPosition(cnv2);
                    double width = mousePos.X - Canvas.GetLeft(draggedRectangle);
                    double height = mousePos.Y - Canvas.GetTop(draggedRectangle);
                    if (height > 0 && width > 0)
                    {
                        draggedRectangle.Width = width;
                        draggedRectangle.Height = height;
                        int widthInt = Convert.ToInt32(width);
                        int heightInt = Convert.ToInt32(height);
                        Ancho_Objeto.Text = Convert.ToString(widthInt / 8);
                        Alto_Objeto.Text = Convert.ToString(heightInt / 8);
                        draggedRectangle.Children.OfType<TextBlock>().First().Width = width;
                        draggedRectangle.Children.OfType<TextBlock>().Last().Height = height;
                    }
                }
            }
        }
        private void Widget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // start dragging
            var rectangle = sender as Grid;
            
            rectangle.CaptureMouse();
            drag = true;

            if(e.ClickCount == 2)
            {
                
                //Condiciones para activar o desactivar partes del menú dependiendo del tipo de objeto seleccionado
                for (int i = 0; i < m_lstLabels.Count(); i++)
                {
                    if (m_lstLabels[i].type == types_items.lbl && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = true;
                        sel_prop_barcode.IsEnabled = false;
                        sel_prop_id.IsEnabled = false;
                        sel_prop_campo.IsEnabled = false;
                        alignment_toolbar.IsEnabled = true;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = false;
                    }
                    if (m_lstLabels[i].type == types_items.barcode && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = false;
                        sel_prop_barcode.IsEnabled = true;
                        sel_prop_id.IsEnabled = false;
                        sel_prop_campo.IsEnabled = false;
                        alignment_toolbar.IsEnabled = false;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = false;
                    }
                    if (m_lstLabels[i].type == types_items.field && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = false;
                        sel_prop_barcode.IsEnabled = false;
                        sel_prop_id.IsEnabled = true;
                        sel_prop_campo.IsEnabled = true;
                        alignment_toolbar.IsEnabled = true;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = false;
                    }
                    if (m_lstLabels[i].type == types_items.box && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = false;
                        sel_prop_barcode.IsEnabled = false;
                        sel_prop_id.IsEnabled = false;
                        sel_prop_campo.IsEnabled = false;
                        alignment_toolbar.IsEnabled = false;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = true;
                    }
                    if (m_lstLabels[i].type == types_items.logo && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = false;
                        sel_prop_barcode.IsEnabled = false;
                        sel_prop_id.IsEnabled = false;
                        sel_prop_campo.IsEnabled = false;
                        alignment_toolbar.IsEnabled = false;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = false;
                    }
                    if (m_lstLabels[i].type == types_items.seal && m_lstLabels[i].widget.Name == rectangle.Name)
                    {
                        sel_prop_text.IsEnabled = false;
                        sel_prop_barcode.IsEnabled = false;
                        sel_prop_id.IsEnabled = true;
                        sel_prop_campo.IsEnabled = false;
                        alignment_toolbar.IsEnabled = false;
                        condition_toolbar.IsEnabled = true;
                        border_thin_selector.IsEnabled = false;
                    }

                }
                //Deselecciona los demás objetos antes de seleccionar el nuevo
                for (int i = 0; i < m_lstLabels.Count(); i++)
                {
                    if(rectangle.Name != m_lstLabels[i].widget.Name)
                    {
                        label_item lbl = m_lstLabels[i];
                        lbl.is_selected = false;
                        lbl.widget.Background = Brushes.Transparent;
                        lbl.widget.Opacity = 1;
                        m_lstLabels[i] = lbl;
                    }    
                }  
                //Selecciona el objeto que coincide con el nombre del elemento clickeado
                for (int i = 0; i < m_lstLabels.Count(); i++)
                {
                    if (rectangle.Name == m_lstLabels[i].widget.Name)
                    {
                        if (m_lstLabels[i].is_selected == false)//Todo lo programado aquí dentro es para hacer cuando se selecciona un objeto
                        {
                            Canvas.SetZIndex(m_lstLabels[i].widget, 1);
                            label_item l_lbl = m_lstLabels[i];

                            l_lbl.is_selected = true;

                            m_lstLabels[i] = l_lbl;

                            rectangle.Background = Brushes.LightGreen;
                            rectangle.Opacity = 0.5;

                            rectangle.IsFocused.Equals(true);
                            int heighInt = Convert.ToInt32(rectangle.Height);
                            int widthInt = Convert.ToInt32(rectangle.Width);
                            size_font_object.Text = Convert.ToString(m_lstLabels[i].font_size);

                            select_type_lbl(l_lbl.textbox_type);
                            select_alignment_object(l_lbl.align);
                            id_update_toolbar(l_lbl.id);
                            select_barcode_type(l_lbl.barcode_type);
                            select_border_thin(Convert.ToInt32(l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness));

                            Ancho_Objeto.Text = Convert.ToString(widthInt / 8);
                            Alto_Objeto.Text = Convert.ToString(heighInt / 8);
                            int leftInt = Convert.ToInt32(Canvas.GetLeft(rectangle));
                            int topInt = Convert.ToInt32(Canvas.GetTop(rectangle));
                            lbl_single_select = m_lstLabels[i].idCount;
                            //Actualiza el valor del textbox para añadir texto
                            Text_tb.Text = m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Text;
                            //Actualiza el valor de los datos de condición
                            cond_value.Text = m_lstLabels[i].cond_value;
                            //Funciones que actualizan los datos de la barra de herramientas de acuerdo al objeto seleccionado
                            select_condition_object(m_lstLabels[i].condition);
                            select_field_cond(m_lstLabels[i].cond_resource);
                            select_font_object(m_lstLabels[i].font);
                            //Actualiza los textbox de posicion
                            Object_X.Text = Convert.ToString(leftInt);                            
                            Object_Y.Text = Convert.ToString(topInt);
                            
                        }
                        else if (m_lstLabels[i].is_selected == true)
                        {
                            label_item l_lbl = m_lstLabels[i];
                            l_lbl.is_selected = false;
                            m_lstLabels[i] = l_lbl;

                            m_lstLabels[i].widget.Background = Brushes.Transparent;
                            rectangle.Opacity = 1;
                            Canvas.SetZIndex(m_lstLabels[i].widget, -1);
                        }  
                    }
                }
            }
        }        
        private void cnv2_MouseMove(object sender, MouseEventArgs e)
        {
            ruler2h.MarkerControlReference = cnv2;
            ruler2v.MarkerControlReference = cnv2;
        }
        private void cnv1_MouseMove(object sender, MouseEventArgs e)
        {
            ruler1h.MarkerControlReference = cnv1;
            ruler1v.MarkerControlReference = cnv1;
        }
        #endregion
        #region Eventos de Menú //Eventos del menú de la derecha
        private void Ancho_Objeto_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            

        }
        private void Alto_Objeto_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
        }        
        private void Object_X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
           
        }
        private void Object_Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        private void TbtMain_Scroll(object sender, ScrollEventArgs e)
        {

        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void LabelHorizontal_Selected(object sender, RoutedEventArgs e)
        {
            
        }
        private void LabelVertical_Selected(object sender, RoutedEventArgs e)
        {
            
        }
        private void Text_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {             
               
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Text = Text_tb.Text;
                }                   
                    
                
            }
        }
        private void document_Horizontal_Selected(object sender, RoutedEventArgs e)
        {
            labelOrientation = "landscape";
        }
        private void document_Vertical_Selected(object sender, RoutedEventArgs e)
        {
            labelOrientation = "portrait";
        }
        private void Document_Width_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            cnv1.Width = Convert.ToInt32(Document_Width.Text) * 8;
            cnv2.Width = Convert.ToInt32(Document_Width.Text) * 8;
            ruler1h.Width = Convert.ToInt32(Document_Width.Text) * 8;
            ruler1h.MaxValue = Convert.ToInt32(Document_Width.Text) * 8;
            ruler2h.Width = Convert.ToInt32(Document_Width.Text) * 8;
            ruler2h.MaxValue = Convert.ToInt32(Document_Width.Text) * 8;
        }
        private void Document_Height_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            cnv1.Height = Convert.ToInt32(Document_Height.Text) * 8;
            cnv2.Height = Convert.ToInt32(Document_Height.Text) * 8;
            ruler1v.Height = Convert.ToInt32(Document_Height.Text) * 8;
            ruler1v.MaxValue = Convert.ToInt32(Document_Height.Text) * 8;
            ruler2v.Height = Convert.ToInt32(Document_Height.Text) * 8;
            ruler2v.MaxValue = Convert.ToInt32(Document_Height.Text) * 8;
        }                
        private void Text_tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Tbt1.IsSelected)
                {
                    Tbt1.Focus();
                }
                else if (Tbt2.IsSelected)
                {
                    Tbt2.Focus();
                }
            }
                
        }
        private void border_thin_2_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.line_width = 2;
                    l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness = 2;
                    m_lstLabels[i] = l_lbl;
                }
            }
        }
        private void border_thin_1_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.line_width = 1;
                    l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness = 1;
                    m_lstLabels[i] = l_lbl;
                    
                }
            }
        }
        private void border_thin_3_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.line_width = 3;
                    l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness = 3;
                    m_lstLabels[i] = l_lbl;
                }
            }
        }
        private void border_thin_4_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.line_width = 4;
                    l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness = 4;
                    m_lstLabels[i] = l_lbl;
                    
                }
            }
        }
        private void border_thin_5_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.line_width = 5;
                    l_lbl.widget.Children.OfType<Rectangle>().First().StrokeThickness = 5;
                    m_lstLabels[i] = l_lbl;
                }
            }
        }
        private void barcode_ean8_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.barcode_type = "ean8";
                    m_lstLabels[i] = l_lbl;

                }
            }
        }
        private void barcode_ean13_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.barcode_type = "ean13";
                    m_lstLabels[i] = l_lbl;

                }
            }
        }
        private void barcode_ean14_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.barcode_type = "ean14";
                    m_lstLabels[i] = l_lbl;

                }
            }
        }
        private void barcode_code39_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.barcode_type = "code39";
                    m_lstLabels[i] = l_lbl;

                }
            }
        }
        private void barcode_code128_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item l_lbl = m_lstLabels[i];
                    l_lbl.barcode_type = "code128";
                    m_lstLabels[i] = l_lbl;

                }
            }
        }
        private void Iz_Arr_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, 0);
                    Canvas.SetTop(m_lstLabels[i].widget, 0); 
                }
            }
        }        
        private void cen_arr_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width / 2 - m_lstLabels[i].widget.Width / 2);
                    Canvas.SetTop(m_lstLabels[i].widget, 0);    
                }
            }
        }        
        private void Der_Arr_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width  - m_lstLabels[i].widget.Width );
                    Canvas.SetTop(m_lstLabels[i].widget, 0);
                }
            }
        }
        private void izq_cen_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, 0);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height / 2 - m_lstLabels[i].widget.Height / 2);
                }
            }
        }
        private void cen_cen_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width / 2 - m_lstLabels[i].widget.Width / 2);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height / 2 - m_lstLabels[i].widget.Height / 2);
                }
            }
        }
        private void der_cen_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width - m_lstLabels[i].widget.Width);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height / 2 - m_lstLabels[i].widget.Height / 2);
                }
            }
        }
        private void izq_ab_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, 0);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height - m_lstLabels[i].widget.Height);
                }
            }
        }
        private void cen_ab_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width / 2 - m_lstLabels[i].widget.Width / 2);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height - m_lstLabels[i].widget.Height);
                }
            }
        }
        private void der_ab_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    Canvas.SetLeft(m_lstLabels[i].widget, cnv1.Width - m_lstLabels[i].widget.Width);
                    Canvas.SetTop(m_lstLabels[i].widget, cnv1.Height - m_lstLabels[i].widget.Height);
                }
            }
        }
        private void align_con_iz_Checked(object sender, RoutedEventArgs e)
        {
            if (align_con_der.IsChecked == true)
            {
                align_con_der.IsChecked = false;
            }
            if (align_con_cen.IsChecked == true)
            {
                align_con_cen.IsChecked = false;
            }
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().TextAlignment = TextAlignment.Left;
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Margin = new Thickness(5, 2, 2, 2);
                    lbl.align = "left";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void align_con_cen_Checked(object sender, RoutedEventArgs e)
        {
            if (align_con_der.IsChecked == true)
            {
                align_con_der.IsChecked = false;
            }
            if (align_con_iz.IsChecked == true)
            {
                align_con_iz.IsChecked = false;
            }
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().TextAlignment = TextAlignment.Center;
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Margin = new Thickness(5, 2, 2, 2);
                    lbl.align = "center";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void align_con_der_Checked(object sender, RoutedEventArgs e)
        {
            if (align_con_cen.IsChecked == true)
            {
                align_con_cen.IsChecked = false;
            }
            if (align_con_iz.IsChecked == true)
            {
                align_con_iz.IsChecked = false;
            }
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().TextAlignment = TextAlignment.Right;
                    m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Margin = new Thickness(0, 2, 0, 2);
                    lbl.align = "right";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_saleform_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.SaleForm";
                    m_lstLabels[i] = lbl;
                }
            }

        }
        private void cond_line_itemName_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.ItemName";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_description_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Description";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_NutInfo_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.NutritionalInfo";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_TraceInfo_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.TraceInfo";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_units_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Units";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_price_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Price";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_weight_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Weight";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_amount_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Amount";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_OffDesc_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.OffDescription";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_OffImp_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.OffImport";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_Pack_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Packingdate";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_Expiring_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.ExpiringDate";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_pref_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.PreferingDate";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_lote_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Lote";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_line_order_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "line.Order";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_total_weight_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "total.Weight";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_total_amount_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "total.Amount";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_total_units_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "total.Units";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_total_operations_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "total.Operations";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_header_text_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_resource = "header.Text";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_none_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.condition = "none";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_same_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.condition = "equal";
                    m_lstLabels[i] = lbl;
                }
            }

        }
        private void cond_dist_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.condition = "not equal";
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void cond_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cond_value = cond_value.Text;
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void font_arial_sel_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.font = "Arial";
                    lbl.widget.Children.OfType<TextBlock>().First().FontFamily = new FontFamily("Arial");

                    m_lstLabels[i] = lbl;
                }
            }

        }
        private void font_calibri_sel_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.font = "Calibri";
                    lbl.widget.Children.OfType<TextBlock>().First().FontFamily = new FontFamily("Calibri");

                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void font_courier_sel_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.font = "Courier New";
                    lbl.widget.Children.OfType<TextBlock>().First().FontFamily = new FontFamily("Courier New");
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void font_times_sel_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.font = "Times New Roman";
                    lbl.widget.Children.OfType<TextBlock>().First().FontFamily = new FontFamily("times New Roman");

                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void size_font_object_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.font_size = (int)size_font_object.Value;
                    lbl.widget.Children.OfType<TextBlock>().First().FontSize = (int)size_font_object.Value;
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void sel_negrita_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.negrita = true;

                    lbl.widget.Children.OfType<TextBlock>().First().FontWeight = FontWeight.FromOpenTypeWeight(700);
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_negrita_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.negrita = false;

                    lbl.widget.Children.OfType<TextBlock>().First().FontWeight = FontWeight.FromOpenTypeWeight(400);
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_cursiva_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = true;
                    lbl.widget.Children.OfType<TextBlock>().First().FontStyle = FontStyles.Italic;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_cursiva_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.widget.Children.OfType<TextBlock>().First().FontStyle = FontStyles.Normal;
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void sel_line_saleform_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].textbox_type == "label")
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.SaleForm";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_itemname_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.ItemName";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_description_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Description";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_nut_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.NutritionalInfo";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_trace_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.TraceInfo";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_units_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Units";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_price_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Price";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_weight_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Weight";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_amount_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Amount";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_offdesc_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.OffDescription";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_offimp_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.OffImport";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;                    
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_pack_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.PackingDate";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;                    
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_exp_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.ExpiringDate";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_pref_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Preferingdate";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_lote_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Lote";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_line_order_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "line.Order";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_total_weight_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "total.Weight";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_total_amount_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "total.Amount";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_total_units_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "total.Units";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_total_operations_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "total.Operations";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }

        private void sel_header_text_Selected(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    lbl.cursiva = false;
                    lbl.textbox_type = "header.Text";
                    lbl.widget.Children.OfType<TextBlock>().First().Text = lbl.textbox_type;
                    m_lstLabels[i] = lbl;
                }
            }
        }
        private void id_type_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].idCount == lbl_single_select && m_lstLabels[i].type == types_items.field)
                {
                    label_item lbl = m_lstLabels[i];
                    if(lbl.textbox_type!="line.Description")
                    {
                        lbl.id = (int)id_type.Value;
                    }
                    
                    m_lstLabels[i] = lbl;
                }
            }
        }

        #endregion
        #region Eventos globales //Eventos a nivel ventana
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if (e.Key == Key.Delete)
            {
                for (int i = 0; i < m_lstLabels.Count(); i++)
                 {
                    if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                    {
                        if (Tbt1.IsSelected)
                        {
                            cnv1.Children.Remove(m_lstLabels[i].widget);

                            m_lstLabels.RemoveAt(i);

                        }
                        else if (Tbt2.IsSelected)
                        {
                            cnv2.Children.Remove(m_lstLabels[i].widget);

                            m_lstLabels.RemoveAt(i);
                        }
                    }
                }


            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
            if (MessageBox.Show(quieres_cerrar.Text, cerrar.Text, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (MessageBox.Show(quieres_guardar.Text, guardar.Text, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    dlg_save.DefaultExt = ".xml";
                    dlg_save.Filter = "XML Files (*.xml)|*.xml";
                    dlg_save.FileOk += Dlg_save_FileOk;
                    dlg_save.FileName = m_nombre_Etiqueta;
                    dlg_save.ShowDialog();
                    
                }
                else
                {
                    
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
        #endregion
        #region Funciones auxiliares //Funciones que ayudan a reducir líneas de código en lugares críticos
        //En concreto las siguientes funciones lo que hacen es seleccionar en el menú de la derecha lo que se les 
        //pasa por parámetros. Esto es especialmente útil en las funciones de seleccionar objeto o creación de objeto.
        //Básicamente para actualizar el valor del menú de la derecha con las propiedades del objeto
        public void select_field_cond(string cond_value) //Función para la seleción de pestaña de campo de condición
        {
            if (cond_value == "line.SaleForm")
            {
                cond_line_saleform.IsSelected = true;
            }
            else if (cond_value == "line.ItemName")
            {
                cond_line_itemName.IsSelected = true;
            }
            else if (cond_value == "line.Description")
            {
                cond_line_description.IsSelected = true;
            }
            else if (cond_value == "line.MutritionalInfo")
            {
                cond_line_NutInfo.IsSelected = true;
            }
            else if (cond_value == "line.TraceInfo")
            {
                cond_line_TraceInfo.IsSelected = true;
            }
            else if (cond_value == "line.Price")
            {
                cond_line_price.IsSelected = true;
            }
            else if (cond_value == "line.Units")
            {
                cond_line_units.IsSelected = true;
            }
            else if (cond_value == "line.Price")
            {
                cond_line_price.IsSelected = true;
            }
            else if (cond_value == "line.Weight")
            {
                cond_line_weight.IsSelected = true;
            }
            else if (cond_value == "line.Amount")
            {
                cond_line_amount.IsSelected = true;
            }
            else if (cond_value == "line.OffDescription")
            {
                cond_line_OffDesc.IsSelected = true;
            }
            else if (cond_value == "line.OffImport")
            {
                cond_line_OffImp.IsSelected = true;
            }
            else if (cond_value == "line.PackingDate")
            {
                cond_line_Pack.IsSelected = true;
            }
            else if (cond_value == "line.ExpiringDate")
            {
                cond_line_Expiring.IsSelected = true;
            }
            else if (cond_value == "line.PreferingDate")
            {
                cond_line_pref.IsSelected = true;
            }
            else if (cond_value == "line.Lote")
            {
                cond_line_lote.IsSelected = true;
            }
            else if (cond_value == "line.Order")
            {
                cond_line_order.IsSelected = true;
            }
            else if (cond_value == "total.Weight")
            {
                cond_total_weight.IsSelected = true;
            }
            else if (cond_value == "total.Amount")
            {
                cond_total_amount.IsSelected = true;
            }
            else if (cond_value == "total.Units")
            {
                cond_total_units.IsSelected = true;
            }
            else if (cond_value == "total.Operations")
            {
                cond_total_operations.IsSelected = true;
            }
            else if (cond_value == "header.Text")
            {
                cond_header_text.IsSelected = true;
            }            
        }
        public void select_font_object(string font)//Función que selecciona la fuente en el menú
        {
            if (font == "Arial")
            {
                font_arial_sel.IsSelected = true;
            }
            else if (font == "Calibri")
            {
                font_calibri_sel.IsSelected = true;
            }
            else if (font == "Courier New")
            {
                font_courier_sel.IsSelected = true;
            }
            else if (font == "Times New Roman")
            {
                font_times_sel.IsSelected = true;
            }
        }
        public void select_type_lbl(string textbox_type)//Función que selecciona el tipo de etiqueta
        {
            if (textbox_type == "line.SaleForm")
            {
                sel_line_saleform.IsSelected = true;
            }
            else if (textbox_type == "line.ItemName")
            {
                sel_line_itemname.IsSelected = true;
            }
            else if (textbox_type == "line.Description")
            {
                sel_line_description.IsSelected = true;
            }
            else if (textbox_type == "line.MutritionalInfo")
            {
                sel_line_nut.IsSelected = true;
            }
            else if (textbox_type == "line.TraceInfo")
            {
                sel_line_trace.IsSelected = true;
            }
            else if (textbox_type == "line.Price")
            {
                sel_line_price.IsSelected = true;
            }
            else if (textbox_type == "line.Units")
            {
                sel_line_units.IsSelected = true;
            }
            else if (textbox_type == "line.Price")
            {
                sel_line_price.IsSelected = true;
            }
            else if (textbox_type == "line.Weight")
            {
                sel_line_weight.IsSelected = true;
            }
            else if (textbox_type == "line.Amount")
            {
                sel_line_amount.IsSelected = true;
            }
            else if (textbox_type == "line.OffDescription")
            {
                sel_line_offdesc.IsSelected = true;
            }
            else if (textbox_type == "line.OffImport")
            {
                sel_line_offimp.IsSelected = true;
            }
            else if (textbox_type == "line.PackingDate")
            {
                sel_line_pack.IsSelected = true;
            }
            else if (textbox_type == "line.ExpiringDate")
            {
                sel_line_exp.IsSelected = true;
            }
            else if (textbox_type == "line.PreferingDate")
            {
                sel_line_pref.IsSelected = true;
            }
            else if (textbox_type == "line.Lote")
            {
                sel_line_lote.IsSelected = true;
            }
            else if (textbox_type == "line.Order")
            {
                sel_line_order.IsSelected = true;
            }
            else if (textbox_type == "total.Weight")
            {
                sel_total_weight.IsSelected = true;
            }
            else if (textbox_type == "total.Amount")
            {
                sel_total_amount.IsSelected = true;
            }
            else if (textbox_type == "total.Units")
            {
                sel_total_units.IsSelected = true;
            }
            else if (textbox_type == "total.Operations")
            {
                sel_total_operations.IsSelected = true;
            }
            else if (textbox_type == "header.Text")
            {
                sel_header_text.IsSelected = true;
            }
        }
        public void select_alignment_object (string align)//Función que selecciona la pestaña que corresponda a la alineación del objeto
        {
            if (align == "left")
            {
                if (align_con_cen.IsChecked == true)
                {
                    align_con_cen.IsChecked = false;
                }
                if (align_con_der.IsChecked == true)
                {
                    align_con_der.IsChecked = false;
                }
                align_con_iz.IsChecked = true;
            }
            if (align == "center")
            {
                if (align_con_iz.IsChecked == true)
                {
                    align_con_iz.IsChecked = false;
                }
                if (align_con_der.IsChecked == true)
                {
                    align_con_der.IsChecked = false;
                }
                align_con_cen.IsChecked = true;
            }
            if (align == "right")
            {
                if (align_con_cen.IsChecked == true)
                {
                    align_con_cen.IsChecked = false;
                }
                if (align_con_der.IsChecked == true)
                {
                    align_con_der.IsChecked = false;
                }
                align_con_iz.IsChecked = true;
            }
        }
        public void select_condition_object (string cond)//Función que selecciona la pestaña dependiendo de la condición del objeto
        {
            if (cond == "none")
            {
                cond_none.IsSelected = true;
            }
            if (cond == "not equal")
            {
                cond_dist.IsSelected = true;
            }
            if (cond == "equal")
            {
                cond_same.IsSelected = true;
            }
        }
        public void id_update_toolbar (int id )//Función para actualizar el id de la toolbox de acuerdo al id del objeto
        {
            id_type.Text = Convert.ToString(id);
        }
        public void select_barcode_type (string p_barcode_type)//Función para seleccionar el código de barras en el menú de la derecha
        {
            if (p_barcode_type == "ean8")
            {
                barcode_ean8.IsSelected = true;
            }
            if (p_barcode_type == "ean13")
            {
                barcode_ean13.IsSelected = true;
            }
            if (p_barcode_type == "ean14")
            {
                barcode_ean14.IsSelected = true;
            }
            if (p_barcode_type == "code39")
            {
                barcode_code39.IsSelected = true;
            }
            if (p_barcode_type == "code128")
            {
                barcode_code128.IsSelected = true;
            }
        }
        public void select_page_orientation(string labelOrientation)//Función para seleccionar la orientación de'página en el menú de la derecha
        {
            if (labelOrientation == "portrait")
            {
                document_Vertical.IsSelected = true;
            }
            else if (labelOrientation == "landscape")
            {
                document_Horizontal.IsSelected = true;
            }
        }
        public void select_border_thin (int thinkness)
        {
            switch (thinkness)
            {
                case 1:
                    border_thin_1.IsSelected = true;
                    break;
                case 2:
                    border_thin_2.IsSelected = true;
                    break;
                case 3:
                    border_thin_3.IsSelected = true;
                    break;
                case 4:
                    border_thin_4.IsSelected = true;
                    break;
                case 5:
                    border_thin_5.IsSelected = true;
                    break;
            }


        }
        #endregion

        private void español_Selected(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionaryManual("es");
            
        }

        private void ingles_Selected(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionaryManual("en");
        }

        private void italiano_Selected(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionaryManual("it");
        }

        private void frances_Selected(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionaryManual("fr");
        }

        private void aleman_Selected(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionaryManual("de");
        }
    }
}
