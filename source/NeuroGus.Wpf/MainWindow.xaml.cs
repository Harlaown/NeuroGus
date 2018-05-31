using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NeuroGus.Core.Model;
using NeuroGus.Core.Parser;
using NeuroGus.Data;

namespace NeuroGus.Wpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 
        private List<ClassifiableText> _classifiableText;
        private static readonly string Path = $@"{Directory.GetCurrentDirectory()}\Files\testdataset.xlsx";
        private  NeuroHandler _neuroHandler = new NeuroHandler();
        public static MainWindow Instance;



        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Text.Text = "Обучение";
                    Text.IsEnabled = false;
                });
             //   Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DbEntities>());
                var characteristics = _neuroHandler.Db.Characteristics.ToList();
                var vocabulary = _neuroHandler.Db.VocabularyWords.ToList();
                if (! _neuroHandler.LoadTrainedClassifiers(characteristics, vocabulary))
                {
                    _classifiableText = ExcelParser.XlsxToClassifiableTexts(Path);
                    vocabulary = _neuroHandler.VocabularySaveToStorage(_classifiableText);
                    characteristics =_neuroHandler.SaveCharacteristicsToStorage(_classifiableText);
                    var classifiableTextForTrain = _neuroHandler. SaveClassifiableTextsToStorage(_classifiableText);
                   _neuroHandler.CreateClassifiers(characteristics, vocabulary);
                   _neuroHandler. TrainAndSaveClassifiers(classifiableTextForTrain);
                   _neuroHandler. CheckClassifiersAccuracy(new FileInfo(Path));
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Text.Text = " ";
                    Text.IsEnabled = true;
                });
            });
        }
                  
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NeuroText.Text = "";
            var text = new ClassifiableText(Text.Text);
            var classifiedCharacteristics = new StringBuilder();
            foreach (var classifier in _neuroHandler._classifiers)
            {
                var classifiedValue = classifier.Classify(text);
                classifiedCharacteristics.Append(classifier.GetCharacteristic().Name)
                    .Append(": ")
                    .Append(classifiedValue.Value)
                    .Append("\n");
                NeuroText.Text += classifiedCharacteristics.ToString();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Database.Delete(_neuroHandler.Db.Database.Connection);
           _neuroHandler.Db.Dispose();
            Environment.Exit(1);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Button_Click(null,null);
        }
    }
}


