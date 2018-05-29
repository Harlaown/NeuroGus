using System;
using System.Collections.Generic;
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
        private readonly DbEntities Db = new DbEntities();
        private List<ClassifiableText> _classifiableText;
        private readonly INGram _nGram = new FilteredUnigram();
        private static readonly string Path = $@"{Directory.GetCurrentDirectory()}\Files\testdataset.xlsx";
        private readonly List<Classifier> _classifiers = new List<Classifier>();

        public MainWindow()
        {
            InitializeComponent();
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
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DbEntities>());
                var characteristics = Db.Characteristics.ToList();
                var vocabulary = Db.VocabularyWords.ToList();
                if (!LoadTrainedClassifiers(characteristics, vocabulary))
                {
                    _classifiableText = ExcelParser.XlsxToClassifiableTexts(Path);
                    vocabulary = VocabularySaveToStorage(_classifiableText);
                    characteristics = SaveCharacteristicsToStorage(_classifiableText);
                    var classifiableTextForTrain = SaveClassifiableTextsToStorage(_classifiableText);
                    CreateClassifiers(characteristics, vocabulary);
                    TrainAndSaveClassifiers(classifiableTextForTrain);
                    CheckClassifiersAccuracy(new FileInfo(Path));
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Text.Text = " ";
                    Text.IsEnabled = true;
                });
            });
        }

        private List<VocabularyWord> VocabularySaveToStorage(List<ClassifiableText> classifiableT)
        {
            AddAllVocabularyWords(new VocabularyBuilder(_nGram).GetVocabulary(classifiableT));
            return Db.VocabularyWords.ToList();
        }

        private bool LoadTrainedClassifiers(ICollection<Characteristic> characteristics,
            List<VocabularyWord> vocabulary)
        {
            if (characteristics.Count == 0 || vocabulary.Count == 0) return false;
            var curdir = Directory.GetCurrentDirectory();
            var curNeuroDir = new DirectoryInfo($@"{curdir}\NeuroGus");
            foreach (var characteristic in characteristics)
            {
                var trainedClassifier =
                    new FileInfo($@"{curNeuroDir.FullName}\{characteristic.Name}NeuralNetworkClassifier");
                if (trainedClassifier.Exists)
                {
                    var classfiler = new Classifier(trainedClassifier, characteristic, vocabulary, _nGram);
                    classfiler.NeuroNetworkMessage += ClassfilerOnNeuroNetworkMessage;
                    _classifiers.Add(classfiler);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void ClassfilerOnNeuroNetworkMessage(object sender, NeuroNetworkEventArgs e)
        {
            SendMessageInForm(e.Message);
        }

        public Characteristic SaveCharacteristic(Characteristic characteristic)
        {
            if (characteristic == null || characteristic.Name.Equals("") || characteristic.PossibleValues == null ||
                characteristic.PossibleValues.Count == 0)
                return null;
            if (FindCharacteristicByName(characteristic.Name) != null)
                throw new ArgumentException("Characteristic already exists");
            var i = 1;
            characteristic.PossibleValues.Remove(null);
            characteristic.PossibleValues.Remove(new CharacteristicValue(""));
            characteristic.PossibleValues = characteristic.PossibleValues.Distinct().ToList();
            foreach (var characteristicValue in characteristic.PossibleValues)
            {
                if (characteristicValue == null) continue;
                characteristicValue.Characteristic = characteristic;
                characteristicValue.CharacteristicId = characteristic.Id;
                characteristicValue.OrderNumber = i++;
            }

            Db.Characteristics.Add(characteristic);
            Db.SaveChanges();
            return characteristic;
        }

        private Characteristic FindCharacteristicByName(string characteristicName)
        {
            var characteristic = (from c in Db.Characteristics where c.Name == characteristicName select c)
                .FirstOrDefault();
            return characteristic;
        }

        private List<Characteristic> SaveCharacteristicsToStorage(IEnumerable<ClassifiableText> classifiableTexts)
        {
            var characteristics = GetCharacteristicsCatalog(classifiableTexts);
            foreach (var characteristic in characteristics)
                try
                {
                    SaveCharacteristic(characteristic);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

            // return Characteristics with IDs
            return Db.Characteristics.ToList();
        }

        private static IEnumerable<Characteristic> GetCharacteristicsCatalog(IEnumerable<ClassifiableText> classifiableTexts)
        {
            var characteristics = new Dictionary<Characteristic, Characteristic>();
            foreach (var classifiableText in classifiableTexts)
                // for all classifiable texts characteristic values
                //
            foreach (var entry in classifiableText.Characteristics)
            {
                // add characteristic to catalog
                if (!characteristics.ContainsKey(entry.Key)) characteristics.Add(entry.Key, entry.Key);

                // add characteristic value to possible values
                characteristics[entry.Key].PossibleValues.Add(entry.Value);
            }

            var temp = new HashSet<Characteristic>(characteristics.Keys.ToList());
            return temp;
        }

        private List<ClassifiableText> SaveClassifiableTextsToStorage(List<ClassifiableText> classifiableTexts)
        {
            try
            {
                SaveAllTexts(classifiableTexts);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            // return classifiable texts from DB
            return classifiableTexts;
        }

        private void CreateClassifiers(IEnumerable<Characteristic> characteristics, List<VocabularyWord> vocabulary)
        {
            foreach (var characteristic in characteristics)
            {
                var classifier = new Classifier(characteristic, vocabulary, _nGram);
                classifier.NeuroNetworkMessage += ClassfilerOnNeuroNetworkMessage;
                _classifiers.Add(classifier);
            }
        }

        private void TrainAndSaveClassifiers(List<ClassifiableText> classifiableTextForTrain)
        {
            var dir = Directory.GetCurrentDirectory();
            var filedir = new DirectoryInfo($@"{dir}\NeuroGus");
            if (!filedir.Exists) filedir.Create();
            foreach (var classifier in _classifiers)
            {
                classifier.Train(classifiableTextForTrain);
                var neuroPath = new FileInfo($@"{filedir.FullName}\{classifier}");
                if (!neuroPath.Exists)
                {
                    var temp = neuroPath.Create();
                    temp.Close();
                }

                classifier.SaveTrainedClassifier(neuroPath);
            }

            Classifier.Shutdown();
        }

        private static List<ClassifiableText> GetClassifiableTexts(FileInfo file)
        {
            var classifiableTexts = new List<ClassifiableText>();
            try
            {
                classifiableTexts = ExcelParser.XlsxToClassifiableTexts(file.FullName);
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
            }

            return classifiableTexts;
        }

        private void CheckClassifiersAccuracy(FileInfo file)
        {
            // read second sheet from a file
            var classifiableTexts = GetClassifiableTexts(file);
            foreach (var classifier in _classifiers)
            {
                var characteristic = classifier.GetCharacteristic();
                var correctlyClassified = 0;
                foreach (var text in classifiableTexts)
                {
                    var idealValue = text.GetCharacteristicValue(characteristic.Name);
                    var classifiedValue = classifier.Classify(text);
                    if (classifiedValue.Value.Equals(idealValue.Value)) correctlyClassified++;
                }

                var accuracy = (double) correctlyClassified / classifiableTexts.Count * 100;
                SendMessageInForm(
                    $@"Accuracy of Classifier for {characteristic.Name} characteristic: {accuracy:0.00}%, accuracy.");
            }
        }

        private void SendMessageInForm(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newTextBlock = new TextBlock {Text = message};
                ListBoxx.Items.Add(newTextBlock);
            });
        }

        public List<ClassifiableText> SaveAllTexts(List<ClassifiableText> classifiableTexts)
        {
            if (classifiableTexts == null || classifiableTexts.Count == 0) return null;
            foreach (var text in classifiableTexts)
            {
                if (text == null || text.Text.Equals("") || text.Characteristics == null ||
                    text.Characteristics.Count == 0)
                    continue;
                if (!FillCharacteristicNamesAndValuesIDs(text))
                    throw new ArgumentException("Characteristic value not exists");
                Db.ClassifiableTexts.Add(text);
                Db.SaveChanges();
            }

            return classifiableTexts;
        }

        private bool FillCharacteristicNamesAndValuesIDs(ClassifiableText text)
        {
            var characteristicValueIsFound = true;
            foreach (var entry in text.Characteristics)
            {
                var characteristic = FindCharacteristicByName(entry.Key.Name);
                if (characteristic == null) characteristicValueIsFound = false;

                // fill characteristic id from DB
                if (characteristic != null)
                {
                    entry.Key.Id = characteristic.Id;
                    entry.CharacteristicsNameId = characteristic.Id;

                    // fill characteristic value id and order number from DB
                    //
                    foreach (var characteristicValue in characteristic.PossibleValues)
                    {
                        if (!characteristicValue.Value.Equals(entry.Value.Value)) continue;
                        entry.Value.Id = characteristicValue.Id;
                        entry.CharacteristicsValueId = characteristicValue.Id;
                        entry.Value.OrderNumber = characteristicValue.OrderNumber;
                    }
                }

                if (!characteristicValueIsFound) return false;
            }

            return true;
        }

        public void AddAllVocabularyWords(List<VocabularyWord> vocabulary)
        {
            if (vocabulary == null || vocabulary.Count == 0) return;
            foreach (var vocabularyWord in new HashSet<VocabularyWord>(vocabulary))
            {
                if (vocabularyWord == null || vocabularyWord.Value.Equals("") ||
                    IsVocabularyWordExistsInDb(vocabularyWord))
                    continue;
                Db.VocabularyWords.Add(vocabularyWord);
                Db.SaveChanges();
            }
        }

        private bool IsVocabularyWordExistsInDb(VocabularyWord vocabularyWord)
        {
            var vocub = (from v in Db.VocabularyWords where v.Value == vocabularyWord.Value select v).FirstOrDefault();
            return vocub != null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NeuroText.Text = "";
            var text = new ClassifiableText(Text.Text);
            var classifiedCharacteristics = new StringBuilder();
            foreach (var classifier in _classifiers)
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
            Database.Delete(Db.Database.Connection);
            Db.Dispose();
            Environment.Exit(1);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Button_Click(null,null);
        }
    }
}


