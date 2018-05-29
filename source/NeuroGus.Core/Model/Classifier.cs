using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Encog;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Persist;

namespace NeuroGus.Core.Model
{
    public class Classifier
    {
        private readonly Characteristic _characteristic;
        private readonly int _inputLayerSize;
        private readonly int _outputLayerSize;
        private readonly BasicNetwork _network;
        private readonly List<VocabularyWord> _vocabulary;
        private readonly INGram _inGram;

        /// <summary>
        /// Событие получения информации об обучении нейросети
        /// </summary>
        public event EventHandler<NeuroNetworkEventArgs> NeuroNetworkMessage;

        public Classifier(FileInfo trainedNetwork, Characteristic characteristic,
            List<VocabularyWord> vocabulary, INGram inGram)
        {
            if (characteristic == null ||
                characteristic.Name.Equals("") ||
                characteristic.PossibleValues == null ||
                characteristic.PossibleValues.Count == 0 ||
                vocabulary == null ||
                vocabulary.Count == 0 ||
                inGram == null)
                throw new ArgumentException();

            this._characteristic = characteristic;
            this._vocabulary = vocabulary;
            _inputLayerSize = vocabulary.Count;
            _outputLayerSize = characteristic.PossibleValues.Count;
            this._inGram = inGram;

            if (trainedNetwork == null)
                _network = CreateNeuralNetwork();
            else
                try
                {
                    _network = (BasicNetwork) EncogDirectoryPersistence.LoadObject(trainedNetwork);
                }
                catch (PersistError)
                {
                    throw new ArgumentException();
                }
        }

        public Classifier(Characteristic characteristic,
            List<VocabularyWord> vocabulary, INGram inGram) : this(null, characteristic, vocabulary,
            inGram)
        {
        }

        public static void Shutdown()
        {
            EncogFramework.Instance.Shutdown();
        }

        private BasicNetwork CreateNeuralNetwork()
        {
            var network = new BasicNetwork();

            // input layer
            network.AddLayer(new BasicLayer(null, true, _inputLayerSize));

            // hidden layer
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, _inputLayerSize / 6));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, _inputLayerSize / 6 / 4));

            // output layer
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, _outputLayerSize));

            network.Structure.FinalizeStructure();
            network.Reset();

            return network;
        }

        public CharacteristicValue Classify(ClassifiableText classifiableText)
        {
            var output = new double[_outputLayerSize];

            // calculate output vector
            _network.Compute(GetTextAsVectorOfWords(classifiableText).ToArray(), output);
            EncogFramework.Instance.Shutdown();

            return ConvertVectorToCharacteristic(output);
        }

        private CharacteristicValue ConvertVectorToCharacteristic(double[] vector)
        {
            var idOfMaxValue = GetIdOfMaxValue(vector);

            // find CharacteristicValue with found Id
            //

            return _characteristic.PossibleValues.FirstOrDefault(c => c.OrderNumber == idOfMaxValue);
        }

        private static int GetIdOfMaxValue(IReadOnlyList<double> vector)
        {
            var indexOfMaxValue = 0;
            var maxValue = vector[0];

            for (var i = 1; i < vector.Count; i++)
                if (vector[i] > maxValue)
                {
                    maxValue = vector[i];
                    indexOfMaxValue = i;
                }

            return indexOfMaxValue + 1;
        }

        public void SaveTrainedClassifier(FileInfo trainedNetwork)
        {
            EncogDirectoryPersistence.SaveObject(trainedNetwork, _network);
        }

        public Characteristic GetCharacteristic()
        {
            return _characteristic;
        }


        public void Train(List<ClassifiableText> classifiableTexts)
        {
            // prepare input and ideal vectors
            // input <- ClassifiableText text vector
            // ideal <- characteristicValue vector
            //

            var input = GetInput(classifiableTexts);
            var ideal = GetIdeal(classifiableTexts);

            // train
            //
            Propagation train = new ResilientPropagation(_network, new BasicMLDataSet(input, ideal));
            train.ThreadCount = 16;
            NeuroNetworkEventArgs neroMessage;
            // todo: throw exception if iteration count more than 1000
            do
            {
                train.Iteration();
                neroMessage = new NeuroNetworkEventArgs
                {
                    Message =
                        $@"Training Classifier for {_characteristic.Name} characteristic. Errors:{train.Error * 100:0.00}%."
                };
                OnNeuroNetworkMessage(neroMessage);
            } while (train.Error > 0.01);

            train.FinishTraining();

            neroMessage = new NeuroNetworkEventArgs
            {
                Message = $@"Classifier for {_characteristic.Name} characteristic trained. Wait..."
            };
            OnNeuroNetworkMessage(neroMessage);
        }

        private double[][] GetInput(IReadOnlyCollection<ClassifiableText> classifiableTexts)
        {
            var input = new double[classifiableTexts.Count][];
            for (var j = 0; j < classifiableTexts.Count; j++) input[j] = new double[_inputLayerSize];

            // convert all classifiable texts to vectors
            //
            var i = 0;
            foreach (var classifiableText in classifiableTexts) input[i++] = GetTextAsVectorOfWords(classifiableText);
            return input;
        }

        private double[][] GetIdeal(IReadOnlyCollection<ClassifiableText> classifiableTexts)
        {
            var ideal = new double[classifiableTexts.Count][];
            for (var j = 0; j < classifiableTexts.Count; j++) ideal[j] = new double[_inputLayerSize];

            // convert all classifiable text characteristics to vectors
            //
            var i = 0;
            foreach (var classifiableText in classifiableTexts)
                ideal[i++] = GetCharacteristicAsVector(classifiableText);
            return ideal;
        }

        // example:
        // count = 5; id = 4;
        // vector = {0, 0, 0, 1, 0}
        private double[] GetCharacteristicAsVector(ClassifiableText classifiableText)
        {
            var vector = new double[_outputLayerSize];
            vector[classifiableText.GetCharacteristicValue(_characteristic.Name).OrderNumber - 1] = 1;
            return vector;
        }

        private double[] GetTextAsVectorOfWords(ClassifiableText classifiableText)
        {
            var vector = new double[_inputLayerSize];

            // convert text to INGram
            var uniqueValues = _inGram.GetNGram(classifiableText.Text);

            // create vector
            //

            foreach (var word in uniqueValues)
            {
                var vw = FindWordInVocabulary(word);

                if (vw != null) vector[vw.Id - 1] = 1;
            }

            return vector;
        }

        private VocabularyWord FindWordInVocabulary(string word)
        {
            try
            {
                return _vocabulary[_vocabulary.IndexOf(new VocabularyWord(word))];
            }
            catch (Exception)
            {
                return null;
            }
        }


        public override string ToString()
        {
            return _characteristic.Name + "NeuralNetworkClassifier";
        }

        protected virtual void OnNeuroNetworkMessage(NeuroNetworkEventArgs e)
        {
            NeuroNetworkMessage?.Invoke(this, e);
        }
    }

}
