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
enum types_items { none = 0, lbl, txtbox, picture, box };
enum types_pictures { logo = 0, seal = 1, barcode = 2, box = 3 };
namespace Encase_XS_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables de objeto
        int lbl_cnt_weight = 0;
        int lbl_cnt_resume = 0;
        struct label_item
        {
            public Grid widget;
            
            //public Label widget;
            public int section_id;
            public string orientation;
            public string font;
            public int font_size;
            public string labelSelec;

            public int id;
            
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
        //struct section_item
        //{
        //    public Panel section;
        //    public bool cond_enable;
        //    public string condition;
        //    public string cond_resource;
        //    public string cond_value;
        //}
        ////
        //List<section_item> m_lstSections = new List<section_item>();
        #endregion
        #region OpenFile
        OpenFileDialog dlg = new OpenFileDialog();
        SaveFileDialog dlg_save = new SaveFileDialog();
       
        

        //OpenFileDialog
        public void OpenFile_Click()
        {
            
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();
            
        }
            #endregion
        #region Variables de Etiqueta
            int m_ancho_Etiqueta;
        int m_alto_Etiqueta;
        string m_nombre_Etiqueta;
        int cnvControl = 0;
        string labelOrientation;
        #endregion
        #region Variables de Control
        bool size = false;
        bool drag = false;
        Point startPoint;
        #endregion
        public MainWindow()
        {            
            InitializeComponent();
        }
        #region Crea etiqueta
        public void Crear_Etiqueta(string m_nombre_Etiqueta, int m_ancho_Etiqueta, int m_alto_Etiqueta)
        {
           
            

            cnv1.Children.Clear();
            cnv2.Children.Clear();
            m_lstLabels.Clear();
            lbl_cnt_weight = 0;
            lbl_cnt_resume = 0;


            
            cnv1.Width = m_ancho_Etiqueta * 8;
            
            cnv2.Width = m_ancho_Etiqueta * 8;
            cnv1.Height = m_alto_Etiqueta * 8;
            
            cnv2.Height = m_alto_Etiqueta * 8;

            cnv1.Background = Brushes.White;
            cnv2.Background = Brushes.White;

            cnv1.HorizontalAlignment = HorizontalAlignment.Left;
            cnv2.HorizontalAlignment = HorizontalAlignment.Left;

            cnv1.Margin.Equals(5);
            cnv2.Margin.Equals(5);

            cnv1.Margin.Top.Equals(5);
            cnv2.Margin.Top.Equals(5);

            cnv1.Name = m_nombre_Etiqueta;
            cnv2.Name = m_nombre_Etiqueta;
            
            ruler1h.Width = cnv1.Width + 6;
            ruler1v.Height = cnv1.Height + 6;
          
            ruler2h.Width = cnv2.Width + 6;
            ruler2v.Height = cnv2.Height + 6;

            ruler1h.MaxValue = cnv1.Width/8;
            ruler1v.MaxValue = cnv1.Height/8;

            ruler2h.MaxValue = cnv2.Width/8;
            ruler2v.MaxValue = cnv2.Height/8;


            Design_Frame1.Children.Add(cnv1);
            Design_Frame2.Children.Add(cnv2);

            Tbt1.IsFocused.Equals(true);


            

            Etiqueta_Alto.Text = m_alto_Etiqueta.ToString();
            Etiqueta_Ancho.Text = m_ancho_Etiqueta.ToString();

        }
        #endregion
        private void Nueva_Etiqueta_Click(object sender, RoutedEventArgs e)
        {
            Nueva_Etiqueta_Ventana nueva_Etiqueta_Ventana = new Nueva_Etiqueta_Ventana(m_ancho_Etiqueta, m_alto_Etiqueta, m_nombre_Etiqueta);
            nueva_Etiqueta_Ventana.Show();
        }

        private void Abrir_Etiqueta_Click(object sender, RoutedEventArgs e)
        {
            //File opener
            OpenFile_Click();

        }

        private void Guardar_Etiqueta_Click(object sender, RoutedEventArgs e)
            
        {
            dlg_save.DefaultExt = ".xml";
            dlg_save.Filter = "XML Files (*.xml)|*.xml";
            dlg_save.FileOk += Dlg_save_FileOk;
            dlg_save.FileName = m_nombre_Etiqueta;
            dlg_save.ShowDialog();         
            
            
        }

        private void Dlg_save_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StreamWriter l_fsXML = new StreamWriter(dlg_save.OpenFile());

            

            string l_strAuxOrientation = "";
            switch (labelOrientation)
            {
                case "vertical":
                    l_strAuxOrientation = "portrait";
                    break;
                case "horizontal":
                    l_strAuxOrientation = "landscape";
                    break;
            }
            l_fsXML.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            l_fsXML.WriteLine("<report page_orientation=\"" + l_strAuxOrientation + "\" page_width=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Width / 8)) + "\" page_height=\"" + Convert.ToString(Decimal.Truncate((decimal)cnv1.Height / 8)) + "\" type=\"label\" name=\"" + m_nombre_Etiqueta + "\" page=\"Custom\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" left_margin=\"0\" version=\"1\" units=\"pixels\" auto_offset=\"1\">");
            l_fsXML.WriteLine("\t<if condition=\"equal\" resource1=\"header.LabelType\" value2=\"WEIGHT\">");
            l_fsXML.WriteLine("\t\t<section name=\"WEIGHT\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Width))) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)(cnv1.Height))) + "\" x=\"" + Convert.ToString(Decimal.Truncate(0)) + "\" y=\"" + Convert.ToString(Decimal.Truncate(0)) + "\">");
            for (int idx = 0; idx < m_lstLabels.Count(); idx++)
            {
                int PosX = (int)Canvas.GetLeft(m_lstLabels[idx].widget);
                int PosY = (int)Canvas.GetTop(m_lstLabels[idx].widget);


                if (m_lstLabels[idx].labelSelec == "weight")
                {
                    if (m_lstLabels[idx].type == types_items.lbl)
                    {
                        l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"center\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"arial\">" + m_lstLabels[idx].widget.Name + "</item>");

                    }
                }                 
            }
            l_fsXML.WriteLine("</section>");
            l_fsXML.WriteLine("</if>");
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
                        l_fsXML.WriteLine("\t\t\t\t<item type=\"label\" x=\"" + Convert.ToString(PosX) + "\" y=\"" + Convert.ToString(PosY) + "\" alignment=\"center\" width=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Width)) + "\" height=\"" + Convert.ToString(Decimal.Truncate((decimal)m_lstLabels[idx].widget.Height)) + "\" font=\"arial\">" + m_lstLabels[idx].widget.Name + "</item>");

                    }
                }
            }
            l_fsXML.WriteLine("</section>");
            l_fsXML.WriteLine("</if>");

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
            if (Tbt1.IsFocused || Tbt2.IsFocused)
            {
                label_item l_lblitAux = new label_item();

                l_lblitAux.widget = new Grid();
                TextBlock tb = new TextBlock();
                Rectangle rect = new Rectangle();

                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 3;
                rect.Stroke = Brushes.Black;
                l_lblitAux.widget.Width = 80;
                l_lblitAux.widget.Height = 80;
                l_lblitAux.orientation = "horizontal";
                l_lblitAux.type = types_items.lbl;
                l_lblitAux.id = -1;

                l_lblitAux.widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
                l_lblitAux.widget.MouseMove += Widget_MouseMove;
                l_lblitAux.widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                l_lblitAux.widget.MouseRightButtonDown += Widget_MouseRightButtonDown;
                l_lblitAux.widget.MouseRightButtonUp += Widget_MouseRightButtonUp;


                Ancho_Objeto.Text = Convert.ToString(l_lblitAux.widget.Width / 8);
                Alto_Objeto.Text = Convert.ToString(l_lblitAux.widget.Height / 8);

                Canvas.SetLeft(l_lblitAux.widget, 0);
                Canvas.SetTop(l_lblitAux.widget, 0);

                if (Tbt1.IsFocused)
                {
                    lbl_cnt_weight++;
                    l_lblitAux.labelSelec = "weight";
                    l_lblitAux.widget.Name = "lbl_weight_" + lbl_cnt_weight;
                    tb.Text = "lbl_weight_" + lbl_cnt_weight;
                    cnv1.Children.Add(l_lblitAux.widget);
                }
                else if (Tbt2.IsFocused)
                {
                    lbl_cnt_resume++;
                    l_lblitAux.labelSelec = "resume";
                    l_lblitAux.widget.Name = "lbl_resume_" + lbl_cnt_resume;
                    tb.Text = "lbl_resume_" + lbl_cnt_resume;
                    cnv2.Children.Add(l_lblitAux.widget);
                }
                l_lblitAux.widget.Children.Add(rect);
                l_lblitAux.widget.Children.Add(tb);

                m_lstLabels.Add(l_lblitAux);
                item_selected l_iItemSelected = new item_selected();

                for (int idx = 0; idx < m_lstLabels.Count; idx++)
                {
                    if (m_lstLabels[idx].widget.Name == l_lblitAux.widget.Name)
                    {
                        l_iItemSelected.idx = idx;
                        l_iItemSelected.type = types_items.lbl;
                        Tamaño_Letra_Objeto.Text = Convert.ToString(m_lstLabels[idx].font_size);
                        //widget_font_cmbbox.SelectedItem = m_lstLabels[idx].font;
                    }
                }
                m_iItemSelected.Add(l_iItemSelected);
            }            
        }

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
                if (Tbt1.IsFocused)
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
                else if (Tbt2.IsFocused)
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
                if (Tbt1.IsFocused)
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
                    }   
                }
                else if (Tbt2.IsFocused)
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
                        if (rectangle.IsFocused.Equals(false))
                        {
                            rectangle.IsFocused.Equals(true);
                            int heighInt = Convert.ToInt32(rectangle.Height);
                            int widthInt = Convert.ToInt32(rectangle.Width);

                            Ancho_Objeto.Text = Convert.ToString(widthInt / 8);
                            Alto_Objeto.Text = Convert.ToString(heighInt / 8);
                            int leftInt = Convert.ToInt32(Canvas.GetLeft(rectangle));
                            int topInt = Convert.ToInt32(Canvas.GetTop(rectangle));

                            Object_X.Text = Convert.ToString(leftInt);
                            Object_Y.Text = Convert.ToString(topInt);
                        }
                        else if (rectangle.IsFocused.Equals(true))
                        {
                            rectangle.IsFocused.Equals(false);
                        }
                        
                        
                    }
                }
                
            }
            
        }



        private void Add_Campo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Cajeado_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Add_Imagen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Sello_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Barcode_Click(object sender, RoutedEventArgs e)
        {

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

        private void Tipo_Weight_Selected(object sender, RoutedEventArgs e)
        {
            Nombre_Tipo.Text = Tipo_Weight.Content.ToString();
            Valor_Cond.Text = Tipo_Resume.Content.ToString();
        }

        private void Tipo_Resume_Selected(object sender, RoutedEventArgs e)
        {
            Nombre_Tipo.Text = Tipo_Resume.Content.ToString();
            Valor_Cond.Text = Tipo_Resume.Content.ToString();
        }

        private void Etiqueta_Ancho_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            cnv1.Width = Convert.ToInt32(Etiqueta_Ancho.Text) * 8;
            cnv2.Width = Convert.ToInt32(Etiqueta_Ancho.Text) * 8;
        }

        private void Etiqueta_Alto_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            cnv1.Height = Convert.ToInt32(Etiqueta_Alto.Text) * 8;
            cnv2.Height = Convert.ToInt32(Etiqueta_Alto.Text) * 8;
            
        }



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
    }
}
