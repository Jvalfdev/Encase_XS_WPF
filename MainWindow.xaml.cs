//
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
enum types_items { none = 0, lbl, txtbox, logo, box, barcode, seal};
namespace Encase_XS_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables de objeto//Variables y listas relacionadas con los objetos que añadimos al canvas

        int lbl_cnt_weight = 0;
        int lbl_cnt_resume = 0;
        int lbl_single_select = 0;        
        struct label_item //Estructura para dar valor a las propiedades de cada objeto
        {
            public Grid widget;

            //public Label widget;
            public int section_id;
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
            
            public bool cond_enable;
            public string condition;
            public string cond_resource;
            public string cond_value;

            public types_items type;
        }
        struct item_selected
        {
            public types_items type;
            public int idx;
        }        
        //Lista para guardar los objetos seleccionados
        List<item_selected> m_iItemSelected = new List<item_selected>();
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
        #endregion
        #region Eventos botones principales
        private void Nueva_Etiqueta_Click(object sender, RoutedEventArgs e)//Abre la Ventana de creación de etiqueta
        {
            Nueva_Etiqueta_Ventana nueva_Etiqueta_Ventana = new Nueva_Etiqueta_Ventana(m_ancho_Etiqueta, m_alto_Etiqueta, m_nombre_Etiqueta);
            nueva_Etiqueta_Ventana.Show();
        }
        private void Abrir_Etiqueta_Click(object sender, RoutedEventArgs e)//Abre el dialogo para abrir un xml
        {
            //File opener
            OpenFile_Click();
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

            string l_strAuxOrientation = "";
            switch (labelOrientation) //Switch para elegir orientación de documento
            {
                case "vertical":
                    l_strAuxOrientation = "portrait";
                    break;
                case "horizontal":
                    l_strAuxOrientation = "landscape";
                    break;
            }
            //Comienzo a escribir cada linea del xml
            l_fsXML.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            l_fsXML.WriteLine("<report page_orientation=\"" + l_strAuxOrientation + "\" page_width=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Width / 8)) + "\" page_height=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Height / 8)) + "\" type=\"label\" name=\"" + m_nombre_Etiqueta + "\" page=\"Custom\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" left_margin=\"0\" version=\"1\" units=\"pixels\" auto_offset=\"1\">");
            if (lbl_cnt_weight > 0)
            {
                l_fsXML.WriteLine("\t<if condition=\"equal\" resource1=\"header.LabelType\" value2=\"WEIGHT\">");
                l_fsXML.WriteLine("\t\t<section name=\"WEIGHT\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" x=\"" + Convert.ToString(Decimal.Truncate(0)) + "\" y=\"" + Convert.ToString(Decimal.Truncate(0)) + "\">");
                for (int idx = 0; idx < m_lstLabels.Count(); idx++) //Escribe en cada linea la información de cada objeto añadido a weight
                {
                    int PosX = (int)Canvas.GetLeft(m_lstLabels[idx].widget);
                    int PosY = (int)Canvas.GetTop(m_lstLabels[idx].widget);
                    if (m_lstLabels[idx].labelSelec == "weight")
                    {
                        if (m_lstLabels[idx].type == types_items.lbl)
                        {
                            l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"center\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\""+ m_lstLabels[idx].font+" "+ m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Children.OfType<TextBlock>().First().Text + "</item>");
                        }
                        if (m_lstLabels[idx].type == types_items.box)
                        {
                            l_fsXML.WriteLine("\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");
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
                    if (m_lstLabels[idx].labelSelec == "resume")
                    {
                        if (m_lstLabels[idx].type == types_items.lbl)
                        {
                            l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"center\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\""+ m_lstLabels[idx].font + " " + m_lstLabels[idx].font_size + "\">" + m_lstLabels[idx].widget.Name + "</item>");

                        }
                        if (m_lstLabels[idx].type == types_items.box)
                        {
                            l_fsXML.WriteLine("\t\t\t<item type=\"shape\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" line_width=\"" + Decimal.Truncate((decimal)m_lstLabels[idx].widget.Children.OfType<Rectangle>().First().StrokeThickness) + "\"></item>");
                        }
                    }
                }
                l_fsXML.WriteLine("</section>");
                l_fsXML.WriteLine("</if>");
            }                 
            l_fsXML.Close();
        }
        private void Add_Design_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Delete_Design_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Add_Texto_Click(object sender, RoutedEventArgs args)
        {
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
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 80;
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 80;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.lbl;
                l_lblitAux.id = -1;//Cuidado con el tema de los ids
                tb.TextWrapping = TextWrapping.Wrap;
                l_lblitAux.is_selected = false;
                l_lblitAux.font = "Arial";
                l_lblitAux.font_size = 14; 
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
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    l_lblitAux.resume_num = lbl_cnt_resume;
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "lbl_resume_" + lbl_cnt_resume;
                    tb.Text = "Text " + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock
                //Se le asigna un número a su variable que controla que número de elemento es en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                border_thin_selector.Visibility = Visibility.Hidden;
                sel_font_size.Visibility = Visibility.Visible;
                sel_font_type.Visibility = Visibility.Visible;

            }            
        }
        private void Add_Campo_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Add_Cajeado_Click(object sender, RoutedEventArgs e)
        {
            if (Tbt1.IsSelected || Tbt2.IsSelected)
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

                l_lblitAux.is_selected = false;
                

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
                m_lstLabels.Add(l_lblitAux);
                
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                border_thin_selector.Visibility = Visibility.Visible;
                sel_font_size.Visibility = Visibility.Hidden;
                sel_font_type.Visibility = Visibility.Hidden;
                



            }
        }
        private void Add_Imagen_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Add_Sello_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Add_Barcode_Click(object sender, RoutedEventArgs e)
        {
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
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 80;
                tb.Width = l_lblitAux.widget.Width;
                l_lblitAux.widget.Height = 80;
                tb.Height = l_lblitAux.widget.Height;
                l_lblitAux.orientation = "horizontal";//Horizontal por defecto
                l_lblitAux.type = types_items.barcode;//Tipo de elemento ||||ES IMPORTANTE PARA EL FUNCIONAMIENTO
                l_lblitAux.id = -1;//Cuidado con el tema de los ids
                l_lblitAux.is_selected = false;
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

                    tb.Text = "Barcode";
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsSelected)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "barcode_resume_" + lbl_cnt_resume;
                    tb.Text = "Barcode";
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);//Se le añade al grid el rectángulo
                l_lblitAux.widget.Children.Add(tb);//Se le añade al grid el textblock
                //Se le asigna un número a su variable que controla que número de elemento es en
                //la aplicacion en general
                l_lblitAux.idCount = m_lstLabels.Count();
                m_lstLabels.Add(l_lblitAux);
                //Mostrar/Ocultar cosas necesarias o innecesarias del menú izquierdo
                border_thin_selector.Visibility = Visibility.Hidden;
                sel_font_size.Visibility = Visibility.Visible;
                sel_font_type.Visibility = Visibility.Visible;
            }
        }
        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Abrir_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Guardar_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Salir_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
        #region Eventos de Control de Objetos en Canvas
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
                for (int i = 0; i < m_lstLabels.Count(); i++)
                {
                    if (rectangle.Name == m_lstLabels[i].widget.Name)
                    {
                        if (m_lstLabels[i].is_selected == false)
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
                            
                            Ancho_Objeto.Text = Convert.ToString(widthInt / 8);
                            Alto_Objeto.Text = Convert.ToString(heighInt / 8);
                            int leftInt = Convert.ToInt32(Canvas.GetLeft(rectangle));
                            int topInt = Convert.ToInt32(Canvas.GetTop(rectangle));
                            lbl_single_select = m_lstLabels[i].idCount;


                            Text_tb.Text = m_lstLabels[i].widget.Children.OfType<TextBlock>().First().Text;
                            
                            
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
        #region Eventos de Menú
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
            labelOrientation = "Horizontal";
        }
        private void LabelVertical_Selected(object sender, RoutedEventArgs e)
        {
            labelOrientation = "Vertical";
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
            labelOrientation = "horizontal";
        }
        private void document_Vertical_Selected(object sender, RoutedEventArgs e)
        {
            labelOrientation = "vertical";
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
        #endregion
        #region Eventos globales
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < m_lstLabels.Count(); i++)
            {
                if (m_lstLabels[i].is_selected == true && m_lstLabels[i].idCount == lbl_single_select)
                {
                    if(Tbt1.IsSelected)
                    {
                        cnv1.Children.Remove(m_lstLabels[i].widget);

                        m_lstLabels.RemoveAt(i);

                    }
                    else if(Tbt2.IsSelected)
                    {
                        cnv2.Children.Remove(m_lstLabels[i].widget);

                        m_lstLabels.RemoveAt(i);
                    }                    
                }
            }
        }
        #endregion
    }
}
