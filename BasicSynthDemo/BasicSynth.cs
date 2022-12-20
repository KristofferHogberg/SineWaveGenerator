using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicSynthDemo
{
    public partial class BasicSynth : Form
    {
        /*  NOTES:
            The audio sample rate is a measurement of the samples per second taken by the system from a continuous digital signal.
            These frequencies are measured in kilohertz (kHz).
            The audio bit depth determines the number of possible amplitude values for each sample. 
        */
        private const int _sampleRate = 44100;
        private const short _bitDepth = 16;

        public BasicSynth()
        {
            InitializeComponent();
        }

        private void BasicSynth_KeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show("What a beautiful sound!");

            short[] wave = new short[_sampleRate];
            // Instantiate a array of bytes to convert each shirt in wave[<short>] into a byte[].
            byte[] binaryWave = new byte[_sampleRate * sizeof(short)];
            float frequency = 440f;

            for (int i = 0; i < _sampleRate; i++)
            {
                // Equation for synthesizing a sine wave for every sample.
                wave[i] = Convert.ToInt16(short.MaxValue * Math.Sin(((Math.PI * 2 * frequency) / _sampleRate) * i));
            }

            /*  Instantiate a MemoryStream to be passed in as a parameter in a BinaryWriter object.
                The binaryWriter writes all necessary parts of the wave file's "chunks" to memory, in order to synthesize a sound.
                Reference: http://soundfile.sapp.org/doc/WaveFormat/
           
                Convert the wave[<short>] into binaryWave[<byte>]
                The Buffer class provides much better performance when manipulating a region of memory containing a primitive type.
                We use the BlockCopy function provided from the Buffer class to perform the conversion.
            */
            Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));

            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);

            short blockAlign = _bitDepth / 8;
            int subChunkTwoSize = _sampleRate * blockAlign;

            // The canonical WAVE format starts with the RIFF header:
            binaryWriter.Write(new[] { 'R', 'I', 'F', 'F' });
            binaryWriter.Write(36 + subChunkTwoSize);
            binaryWriter.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

            // The "WAVE" format consists of two sub chunks: "fmt " and "data":
            // The "fmt " sub chunk describes the sound data's format:
            binaryWriter.Write(16);
            binaryWriter.Write((short)1);
            binaryWriter.Write((short)1);
            binaryWriter.Write(_sampleRate);
            binaryWriter.Write(_sampleRate * blockAlign);
            binaryWriter.Write(blockAlign);
            binaryWriter.Write(_bitDepth);

            // The "data" sub chunk contains the size of the data and the actual sound:
            binaryWriter.Write(new[] { 'd', 'a', 't', 'a' });
            binaryWriter.Write(subChunkTwoSize);
            // Here we need to split each short in the wave[<short>] to a byte and add to a byte[<byte>].
            // See notes at the top. 
            binaryWriter.Write(binaryWave);

            // Instantiate a SoundPlayer to play the synthesised sine wave from the memoryStream.
            // we set the memoryStream to 0 to ensure it plays from the start.
            memoryStream.Position = 0;
            new SoundPlayer(memoryStream).Play();
        }
    }
}