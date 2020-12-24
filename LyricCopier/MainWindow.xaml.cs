using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;

namespace LyricCopier
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UserText             { get; set; }
        public int    LivestreamDelayInSec { get; set; } = 10;

        public bool WaitForInsertKey { get; set; } = true;
        public bool WaitForXSeconds  { get; set; }

        public InputSimulator InputSimulator { get; } = new InputSimulator();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Parse text into chunks for each slide
            var textChunks = GetTextChunks();

            // Change focus to Livestream Studio
            //ChangeFocusToLivestream();


            // Delay before pasting text to give the user a chance to get into edit mode
            if (WaitForInsertKey)
                while (InputSimulator.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.INSERT) == false)
                    Thread.Yield();

            else if (WaitForXSeconds)
                Thread.Sleep((int) TimeSpan
                    .FromSeconds(LivestreamDelayInSec)
                    .TotalMilliseconds);
            

            // Paste text into Livestream
            PasteText(textChunks);
        }

        private List<List<string>> GetTextChunks()
        {
            var chunks = new List<List<string>>();
            var currentChunk = new List<string>();

            chunks.Add(currentChunk);

            foreach (var line in UserText.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Currently not worried about multiple consecutive blank lines causing extra spacing
                    currentChunk = new List<string>();
                    chunks.Add(currentChunk);
                }
                else
                {
                    currentChunk.Add(line);
                }

            return chunks;
        }

        private void ChangeFocusToLivestream()
        {
            InputSimulator.Keyboard.ModifiedKeyStroke(
                VirtualKeyCode.MENU, // ALT is named MENU (?)
                VirtualKeyCode.TAB);
        }

        private void PasteText(List<List<string>> textChunks)
        {
            foreach (var chunk in textChunks)
            {
                // Put the chunk on the clipboard
                Clipboard.SetText(string.Join(Environment.NewLine, chunk));

                Thread.Sleep(50);
                if (InputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.ESCAPE))
                    return;

                // Paste the chunk
                InputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

                // Move to the next row
                InputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);

                Thread.Sleep(50);
            }
        }
    }
}