using System.Speech.Synthesis;

namespace ShopErp.App.Service
{
    public class Speaker
    {
        private static SpeechSynthesizer synth = new SpeechSynthesizer();

        public static void Speak(string worlds)
        {
            try
            {
                synth.Speak(worlds);
            }
            catch
            {
            }
        }
    }
}