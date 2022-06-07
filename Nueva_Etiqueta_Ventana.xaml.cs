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
using System.Windows.Shapes;

namespace Encase_XS_WPF
{
    /// <summary>
    /// Interaction logic for Nueva_Etiqueta_Ventana.xaml
    /// </summary>
    
    
    public partial class Nueva_Etiqueta_Ventana : Window
    {
        int m_ancho_Etiqueta;
        int m_alto_Etiqueta;
        string m_nombre_Etiqueta;
        public Nueva_Etiqueta_Ventana(int p_ancho_Etiqueta, int p_alto_Etiqueta, string p_nombre_Etiqueta)
        {
            InitializeComponent();
            m_ancho_Etiqueta = p_ancho_Etiqueta;
            m_alto_Etiqueta = p_alto_Etiqueta;
            m_nombre_Etiqueta = p_nombre_Etiqueta;
            
        }
        
        private void Aceptar_nueva_etiqueta_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Design_Frame1.Children.Clear();
            ((MainWindow)Application.Current.MainWindow).Design_Frame2.Children.Clear();
            m_nombre_Etiqueta = Nueva_Etiqueta_Nombre.Text;
            m_ancho_Etiqueta = Convert.ToInt32(Ancho_Nueva_Etiqueta_WND.Text);
            m_alto_Etiqueta = Convert.ToInt32(Alto_Nueva_Etiqueta_WND.Text);
            ((MainWindow)Application.Current.MainWindow).Crear_Etiqueta(m_nombre_Etiqueta, m_ancho_Etiqueta, m_alto_Etiqueta);
            this.Hide();
        }

       

        private void Cancelar_nueva_etiqueta_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
