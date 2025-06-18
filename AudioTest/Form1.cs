using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using FFMpegCore;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.WaveFormRenderer;
using MediaFile = MediaToolkit.Model.MediaFile;

namespace AudioTest
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            WhileDragging();
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "D:\\VS Projects\\AudioTest\\AudioTest\\bin", TemporaryFilesFolder = "/tmp" });
        }
        public static string baseDir = "D:/VS Projects/AudioTest/AudioTest/Audio";
        //string SelectedFile = baseDir + "/Ats p4 test.mp4";
        string SelectedFile = "";
        //string SelectedFile = "C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/P2.mp4";
        
        private async void ParseAudio(float matchOverride = 0.95f)
        {
            bool NewMethod = true;
            AudioFileReader SourceFile = new AudioFileReader(SelectedFile);
            if (!NewMethod)
            {
                //Split into 4 parts.
                using (Engine e = new Engine())
                {
                    MediaFile media = new MediaFile(SelectedFile);
                    ConversionOptions options = new ConversionOptions();

                    e.GetMetadata(media);
                    MediaFile part1 = new MediaFile(baseDir + "/Work Segments/Part1.mp4");
                    MediaFile part2 = new MediaFile(baseDir+ "/Work Segments/Part2.mp4");
                    MediaFile part3 = new MediaFile(baseDir + "/Work Segments/Part3.mp4");
                    MediaFile part4 = new MediaFile(baseDir+ "/Work Segments/Part4.mp4");

                    //Setup
                    double milliHalf = media.Metadata.Duration.TotalMilliseconds / 2;
                    double millitQuart = milliHalf / 2;
                    double milliThreeFour = milliHalf + milliHalf / 2;

                    //1
                    options.CutMedia(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(millitQuart));
                    e.Convert(media, part1, options);
                    //2
                    options.CutMedia(TimeSpan.FromMilliseconds(millitQuart), TimeSpan.FromMilliseconds(millitQuart));
                    e.Convert(media, part2, options);
                    //3
                    options.CutMedia(TimeSpan.FromMilliseconds(milliHalf), TimeSpan.FromMilliseconds(millitQuart));
                    e.Convert(media, part3, options);
                    //4
                    options.CutMedia(TimeSpan.FromMilliseconds(milliThreeFour), TimeSpan.FromMilliseconds(media.Metadata.Duration.TotalMilliseconds - milliThreeFour));
                    e.Convert(media, part4, options);

                    //Load Samples

                    Samples.Clear();
                    foreach (string fName in Directory.EnumerateFiles(baseDir + "/Samples"))
                    {
                        SampleData sd = new SampleData();
                        AudioFileReader afr = new AudioFileReader(fName);
                        if (FullSpectroComparison)
                        {
                            int w = (int)Math.Floor(afr.TotalTime.TotalMilliseconds * PixelsPerMillisecond);
                            sd.Spectrogram = CreateSpectro(w, afr);
                            sd.bitmap = new Bitmap(sd.Spectrogram);
                            sd.bHeight = sd.bitmap.Height;
                            sd.bWidth = sd.bitmap.Width;
                            sd.GenerateColorArray();
                        }
                        else
                            sd.Spectrogram = CreateSpectro(SpectroWidth, afr);

                        sd.Length = afr.Length;
                        Samples.Add(sd);
                    }

                    Task<float> exec1 = Task.Run(() => LoopThroughSegment(part1.Filename, matchOverride, 1, label2));
                    Task<float> exec2 = Task.Run(() => LoopThroughSegment(part2.Filename, matchOverride, 2, label2));
                    Task<float> exec3 = Task.Run(() => LoopThroughSegment(part3.Filename, matchOverride, 3, label2));
                    Task<float> exec4 = Task.Run(() => LoopThroughSegment(part4.Filename, matchOverride, 4, label2));

                    await Task.WhenAll(exec1, exec2, exec3, exec4);
                }
            }
            else
            {
                //Load Samples
                Samples.Clear();
                foreach (string fName in Directory.EnumerateFiles(baseDir + "/Samples"))
                {
                    SampleData sd = new SampleData();
                    AudioFileReader afr = new AudioFileReader(fName);
                    if (FullSpectroComparison)
                    {
                        int w = (int)Math.Floor(afr.TotalTime.TotalMilliseconds * PixelsPerMillisecond);
                        sd.Spectrogram = CreateSpectro(w, afr);
                        sd.bitmap = new Bitmap(sd.Spectrogram);
                        sd.bHeight = sd.bitmap.Height;
                        sd.bWidth = sd.bitmap.Width;
                        sd.GenerateColorArray();
                    }
                    else
                        sd.Spectrogram = CreateSpectro(SpectroWidth, afr);

                    sd.Length = afr.Length;
                    Samples.Add(sd);
                }

                //Debug

                //New Parsing Method
                double fileQ = Math.Floor(SourceFile.TotalTime.TotalMilliseconds/4);
                double e1Start = 0;
                double e1End = e1Start+fileQ;
                double e2Start = e1End + 1;
                double e2End = e2Start + fileQ;
                double e3Start = e2End + 1;
                double e3End = e3Start + fileQ;
                double e4Start = e3End + 1;
                double e4End = SourceFile.TotalTime.TotalMilliseconds;

                //Create Tasks to go over 10000 Milliseconds of video
                double TaskTime = 60000;
                double totalTimeElapsed = 0;
                int runningTasks = 0;
                int totalTasks = 0;
                List<Task<float>> TaskList = new List<Task<float>>();
                while (totalTimeElapsed< SourceFile.TotalTime.TotalMilliseconds)
                {
                    if (runningTasks < 4 && totalTimeElapsed<SourceFile.TotalTime.TotalMilliseconds)
                    {
                        Label l = new Label();
                        l.Text = "Task " + totalTasks + " loading..";
                        object[] mVars = new object[3];
                        mVars[0] = LabelPanel;
                        mVars[1] = l;
                        mVars[2] = totalTasks;
                        LabelPanel.BeginInvoke(new PanelAddLabelDelegate(DelegateAddLabel), mVars);
                        double tStart = totalTimeElapsed;
                        double tEnd = totalTimeElapsed + TaskTime;
                        if (tEnd >= SourceFile.TotalTime.TotalMilliseconds)
                            tEnd = SourceFile.TotalTime.TotalMilliseconds;
                        totalTimeElapsed += TaskTime;
                        Task<float> newTask = Task.Run(() => LoopThroughTask2(SourceFile.FileName, matchOverride, totalTasks, l, tStart, tEnd));
                        TaskList.Add(newTask);
                        await Task.Delay(1000).ConfigureAwait(false);
                        runningTasks++;
                        totalTasks++;
                    }
                    else
                    {
                        await Task.WhenAny(TaskList).ConfigureAwait(false);
                        //Remove any completed ones
                        List<Task<float>> complist = new List<Task<float>>();
                        foreach (Task<float> t in TaskList)
                            if (t.IsCompleted)
                                complist.Add(t);
                        foreach (Task<float> t in complist)
                            TaskList.Remove(t);
                        runningTasks--;
                    }
                }
                await Task.WhenAll(TaskList).ConfigureAwait(false);
                //Task<float> exec1 = Task.Run(() => LoopThroughTask2(SourceFile.FileName, matchOverride, 1, label1, e1Start, e1End));
                //Task<float> exec2 = Task.Run(() => LoopThroughTask2(SourceFile.FileName, matchOverride, 2, label4, e2Start, e2End));
                //Task<float> exec3 = Task.Run(() => LoopThroughTask2(SourceFile.FileName, matchOverride, 3, label5, e3Start, e3End));
                //Task<float> exec4 = Task.Run(() => LoopThroughTask2(SourceFile.FileName, matchOverride, 4, label6, e4Start, e4End));

                //await Task.WhenAll(exec1, exec2, exec3, exec4);
            }
        }

        
        //Loop Through vars
        int SpectroWidth = 400;
        int IndividualSegThresh = 500;
        int IterationStepCount = 5;
        int PixelsPerMillisecond = 1;
        float MatchThreshold = 0.95f;
        bool FullSpectroComparison = true;
        SampleData lastSample = null;
        List<SampleData> Samples = new List<SampleData>();


        bool Matching = false;
        private async Task<float> LoopThroughTask2(string fileName, float MatchOverride, int taskIndex, Label info, double StartIndex, double EndIndex)
        {
            //Create a TempFile
            string LocalFileName = fileName;
            int localIndex = taskIndex;
            MediaFile smf = new MediaFile(fileName);
            using (Engine e1 = new Engine())
            {
                //Setup
                ConversionOptions options = new ConversionOptions();
                e1.GetMetadata(smf);

                //Clear File
                string taskFile = baseDir + "/Base Segments/t-" + localIndex + ".mp4";
                File.Delete(taskFile);
                MediaFile tmf = new MediaFile(taskFile);
                options.CutMedia(TimeSpan.FromMilliseconds(StartIndex), TimeSpan.FromMilliseconds(EndIndex - StartIndex));
                e1.Convert(smf, tmf, options);
                LocalFileName = tmf.Filename;
            }
            //Gather the Sample Spectrograms
            MatchThreshold = MatchOverride;
            int SavedSegments = 0;
            AudioFileReader localSource = new AudioFileReader(LocalFileName);
            //Generate Spectrogram
            int w = (int)Math.Floor(localSource.TotalTime.TotalMilliseconds * PixelsPerMillisecond);
            Image FullSpectrogram = CreateSpectro(w, localSource);
            Bitmap FullSpectrogramBitmap = new Bitmap(FullSpectrogram);
            double i = 0;
            double trailingIndex = i;
            //Loop
            while (i < (EndIndex * PixelsPerMillisecond) - (StartIndex * PixelsPerMillisecond))
            {
                //Check 
                foreach (SampleData sd in Samples)
                {
                    //Check for size
                    if (sd.bWidth + i >= FullSpectrogramBitmap.Width)
                    {
                        //Update Text and Abort
                        object[] finalUpdateVars = new object[2];
                        finalUpdateVars[0] = info;
                        finalUpdateVars[1] = "Task " + localIndex + " Completed";
                        info.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), finalUpdateVars);
                        return 1;
                    }
                    //
                    Rectangle r = new Rectangle((int)i, 0, int.Parse("" + sd.bWidth), int.Parse("" + sd.bHeight));
                    using (Bitmap clone = FullSpectrogramBitmap.Clone(r, FullSpectrogramBitmap.PixelFormat))
                    {
                        //pictureBox2.Image = sd.bitmap;
                        //pictureBox1.Image = clone;
                        
                        int similar = 0;
                        Random rand = new Random();
                        similar = ComparePartialImage(clone, sd.ColorArray);

                        /*/New Method
                        try
                        {
                            while (Matching)
                            {
                                int d = rand.Next(2000);
                                await Task.Delay(d);
                            }
                            Matching = true;
                            similar = sd.GetNextMatch(clone);
                            Matching = false;
                        }
                        catch (Exception e)
                        {
                            string s4 = e.Message;
                        }
                        */
                        float perc = (float)similar / ((float)sd.bHeight * sd.bWidth);
                        if (perc >= MatchThreshold)
                        {
                            //match found
                            //Set the offsets
                            int offset = 500 * PixelsPerMillisecond;
                            double startInd = i - offset;
                            double endInd = i + offset + (int)Math.Round(sd.bWidth);
                            //Clamp
                            if (startInd < trailingIndex)
                                startInd = trailingIndex;
                            if (endInd > EndIndex-StartIndex)
                                endInd = EndIndex-StartIndex;
                            //Cut out the segment to be examined, saving the indexes
                            //Get the first segment, and save as an mp4
                            ConversionOptions options = new ConversionOptions();
                            using (Engine e = new Engine())
                            {
                                //Re-Adjust to the Full file scaling
                                startInd += StartIndex;
                                endInd += StartIndex;
                                //Setup
                                e.GetMetadata(smf);
                                MediaFile workSeg = new MediaFile(baseDir + "/Work Segments/" + startInd/PixelsPerMillisecond + "-" + endInd/PixelsPerMillisecond + ".mp4");
                                //Work Segment
                                //Convert startInd to milli, then set the duration to be the difference between the end and start
                                options.CutMedia(TimeSpan.FromMilliseconds(startInd / PixelsPerMillisecond), TimeSpan.FromMilliseconds((endInd / PixelsPerMillisecond) - (startInd / PixelsPerMillisecond)));
                                e.Convert(smf, workSeg, options);

                                //await FFMpeg.SubVideoAsync(FullFile.Filename, baseDir + "/Work Segments/" + taskIndex + "-" + SavedSegments + "-" + (SavedSegments + 1) + ".mp4", TimeSpan.FromMilliseconds(startInd / PixelsPerMillisecond), TimeSpan.FromMilliseconds((endInd / PixelsPerMillisecond)));
                                //Reset Variables
                                i = endInd+1 - StartIndex;
                                trailingIndex = i;
                            }

                            //Reset the match array
                            sd.MatchArray.Clear();

                            //Incrament saved segments
                            SavedSegments++;
                            //Dispose of Objects

                            //Potential Matches found: 0
                            object[] mVars = new object[2];
                            mVars[0] = label2;
                            mVars[1] = "Potential Matches found: " + Directory.GetFiles(baseDir + "/Work Segments").Count();
                            label2.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
                        }
                    }
                }
                //Progress
                i += IterationStepCount;
                double progress = (EndIndex - StartIndex - i) / PixelsPerMillisecond;
                string s = "Task " + localIndex + " Time Remaining: " + (progress / 1000) + " seconds";
                object[] vars = new object[2];
                vars[0] = info;
                vars[1] = s;
                info.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), vars);
            }
            return 1;
        }

        
        private async Task<float> LoopThroughSegment(string fileName, float MatchOverride, int taskIndex, Label info)
        {
            //Gather the Sample Spectrograms
            string LocalFileName = fileName;
            MatchThreshold = MatchOverride;
            int SavedSegments = 0;
            AudioFileReader localSource = new AudioFileReader(LocalFileName);

            //Loop through the array, and find matches
            if (!FullSpectroComparison)
            {
                /*Standard Comparison Methods
                int i = 0;
                while (i < array.Length)
                {
                    //Create a Spectrogram from this segment, and Compare it
                    foreach (SampleData sd in Samples)
                    {
                        byte[] segToCheck = new byte[sd.Length];
                        Array.Copy(array, i, segToCheck, 0, sd.Length);
                        //Method 1 - Array Comparison


                        /*Method 2 - Spectro Comparison
                        File.Create("empty.mp3").Close();
                        BinaryWriter bw = new BinaryWriter(File.OpenWrite("empty.mp3"));
                        bw.Write(segToCheck);
                        bw.Close();
                        AudioFileReader afr = new AudioFileReader("empty.mp3");
                        Image SegSpectro = CreateSpectro(SpectroWidth, afr);

                        //Compare Spectrogram
                        int similarCount = CompareImages(SegSpectro, sd.Spectrogram);
                        float perc = similarCount / (sd.Spectrogram.Width * sd.Spectrogram.Height);
                        if (perc >= MatchThreshold)
                        {
                            //match found
                            int sea = 0;
                        }
                        //
                    }

                    //Incrament
                    i += IterationStepCount;
                    float progress = i / array.Length;
                    label1.Text = "Progress: " + progress * 100 + "%";
                }
                /*/
            }
            //Generate Full Spectrogram, and sample spectrograms to same scale
            else
            {
                //Generate Spectrogram
                int w = (int)Math.Floor(localSource.TotalTime.TotalMilliseconds * PixelsPerMillisecond);
                Image FullSpectrogram = CreateSpectro(w, localSource);
                Bitmap FullSpectrogramBitmap = new Bitmap(FullSpectrogram);
                int i = 0;
                float highestMatch = 0;
                float lowestMatch = 1;
                int StartTrailingIndex = 0;
                while (i < FullSpectrogramBitmap.Width)
                {

                    //Check 
                    foreach (SampleData sd in Samples)
                    {
                        //Check for size
                        if (sd.bWidth + i >= FullSpectrogramBitmap.Width)
                        {
                            //Save the final segment and abort
                            MediaFile FullFile = new MediaFile(LocalFileName);
                            ConversionOptions options = new ConversionOptions();
                            using (Engine e = new Engine())
                            {
                                e.GetMetadata(FullFile);
                                MediaFile finalSeg = new MediaFile(baseDir + "/Base Segments/" + taskIndex + "-" + SavedSegments + ".mp4");
                                options.CutMedia(TimeSpan.FromMilliseconds(StartTrailingIndex / PixelsPerMillisecond), TimeSpan.FromMilliseconds(FullFile.Metadata.Duration.TotalMilliseconds - (StartTrailingIndex / PixelsPerMillisecond)));
                                e.Convert(FullFile, finalSeg, options);
                                object[] finalUpdateVars = new object[2];
                                finalUpdateVars[0] = info;
                                finalUpdateVars[1] = "Completed";
                                info.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), finalUpdateVars);
                                return 1;
                            }
                        }
                        //
                        Rectangle r = new Rectangle(i, 0, int.Parse("" + sd.bWidth), int.Parse("" + sd.bHeight));
                        using (Bitmap clone = FullSpectrogramBitmap.Clone(r, FullSpectrogramBitmap.PixelFormat))
                        {
                            //pictureBox2.Image = sd.bitmap;
                            //pictureBox1.Image = clone;
                            
                            int similar = ComparePartialImage(clone, sd.ColorArray);

                            float perc = (float)similar / ((float)sd.bHeight * sd.bWidth);
                            if (perc >= MatchThreshold)
                            {
                                //match found
                                //Set the offsets
                                int offset = 500 * PixelsPerMillisecond;
                                int startInd = i - offset;
                                int endInd = i + offset + (int)Math.Round(sd.bWidth);
                                //Clamp
                                if (startInd < StartTrailingIndex)
                                    startInd = StartTrailingIndex;
                                if (endInd > FullSpectrogram.Width)
                                    endInd = FullSpectrogram.Width;
                                //Subdivide into 3 segments. the prior, the clip, and the new main;
                                //Get the first segment, and save as an mp4
                                MediaFile FullFile = new MediaFile(LocalFileName);
                                ConversionOptions options = new ConversionOptions();
                                using (Engine e = new Engine())
                                {
                                    e.GetMetadata(FullFile);
                                    MediaFile workSeg = new MediaFile(baseDir + "/Work Segments/" + taskIndex + "-"+ SavedSegments + "-" + (SavedSegments + 1) + ".mp4");
                                    MediaFile oldSeg = new MediaFile(baseDir + "/Base Segments/" + taskIndex + "-" + SavedSegments + ".mp4");

                                    //Previous Segment
                                    //Might need to decrement by 1.
                                    if ((startInd / PixelsPerMillisecond) - (StartTrailingIndex / PixelsPerMillisecond) > 0)
                                    {
                                        
                                        options.CutMedia(TimeSpan.FromMilliseconds(StartTrailingIndex / PixelsPerMillisecond), TimeSpan.FromMilliseconds((startInd / PixelsPerMillisecond)-(StartTrailingIndex/PixelsPerMillisecond)));
                                        e.Convert(FullFile, oldSeg, options);
                                        
                                        //await FFMpeg.SubVideoAsync(FullFile.Filename, baseDir + "/Base Segments/" + taskIndex + "-" + SavedSegments + ".mp4", TimeSpan.FromMilliseconds(StartTrailingIndex / PixelsPerMillisecond), TimeSpan.FromMilliseconds((startInd / PixelsPerMillisecond) - 1));
                                    }

                                    //Work Segment
                                    //Convert startInd to milli, then set the duration to be the difference between the end and start
                                    
                                    options.CutMedia(TimeSpan.FromMilliseconds((startInd / PixelsPerMillisecond)+1), TimeSpan.FromMilliseconds((endInd / PixelsPerMillisecond) - (startInd / PixelsPerMillisecond)));
                                    e.Convert(FullFile, workSeg, options);
                                    
                                    //await FFMpeg.SubVideoAsync(FullFile.Filename, baseDir + "/Work Segments/" + taskIndex + "-" + SavedSegments + "-" + (SavedSegments + 1) + ".mp4", TimeSpan.FromMilliseconds(startInd / PixelsPerMillisecond), TimeSpan.FromMilliseconds((endInd / PixelsPerMillisecond)));
                                    //Reset Variables
                                    i = endInd+1;
                                    StartTrailingIndex = endInd+1;
                                }

                                //Incrament saved segments
                                SavedSegments++;
                                //Dispose of Objects
                                
                                //Potential Matches found: 0
                                object[] mVars = new object[2];
                                mVars[0] = label2;
                                mVars[1] = "Potential Matches found: " + Directory.GetFiles(baseDir + "/Work Segments").Count();
                                label2.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
                            }
                            if (perc > highestMatch)
                                highestMatch = perc;
                            if (perc < lowestMatch)
                                lowestMatch = perc;
                        }
                    }
                    //Progress
                    i += IterationStepCount;
                    float progress = ((float)FullSpectrogramBitmap.Width - i)/PixelsPerMillisecond;
                    string s = "Task " + taskIndex + " Time Remaining: " + (progress / 1000) + " seconds";
                    object[] vars = new object[2];
                    vars[0] = info;
                    vars[1] = s;
                    info.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), vars);
                }
            }
            return 0;
        }

        public delegate void PanelAddPanelDelegate(Panel target, Panel newControl);
        private void DelegateAddPanel(Panel p, Panel n)
        {
            p.Controls.Add(n);
        }

        public delegate void PanelAddLabelDelegate(Panel target, Label myControl, int mult);
        private void DelegateAddLabel(Panel p,Label l, int m)
        {
            p.Controls.Add(l);
            l.SetBounds(25, 30 * m, 400, 25);
        }

        public delegate void TextUpdateDelegate(Label myControl, string myArg2);
        private void DelegateTextUpdate(Label l, string s)
        {
            l.Text = s;
        }

        public delegate void ImageUpdateDelegate(PictureBox myControl, Image myArg2);
        private void DelegateImageUpdate(PictureBox p, Image b)
        {
            try
            {
                p.Image = b;
            }
            catch (Exception e)
            {

            }
        }

        private long ConvertMilliToByteIndex(int milli, int maxMilli, long maxByte)
        {
            float perc = milli / maxMilli;
            long ByteIndex = (long)Math.Ceiling(perc * maxByte);
            return ByteIndex;
        }

        private int ComparePartialImage(Bitmap fullImage, Color[,] partialImage)
        {
            int count = 0;
            for (int i = 0; i < fullImage.Width; i++)
                for (int j = 0; j < fullImage.Height; j++)
                {
                    if (partialImage[i, j] == fullImage.GetPixel(i, j))
                        count++;
                }
            return count;
        }

        private int CompareImages(Image img1, Image img2)
        {
            int count = 0;
            Bitmap bm1 = new Bitmap(img1);
            Bitmap bm2 = new Bitmap(img2);
            for (int i = 0;i<bm1.Width;i++)
                for (int j = 0;j<bm1.Height;j++)
                {
                    if (bm1.GetPixel(i, j) == bm2.GetPixel(i, j))
                        count++;
                }
            return count;
        }

        private Image CreateSpectro(int width, string filename)
        {
            AudioFileReader afr = new AudioFileReader(filename);
            return CreateSpectro(width, afr);
        }

        private Image CreateSpectro(int width, AudioFileReader reader)
        {
            /* Before Call
             
            string FileName = baseDir + "/Test.mp3";
            MediaFoundationReader wf = new MediaFoundationReader(FileName);
            double audioLength = wf.TotalTime.TotalSeconds;
            int width = (int)(wf.TotalTime.TotalSeconds * 200);
            */

            //Init
            MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
            RmsPeakProvider rmsPeakProvider = new RmsPeakProvider(200); // e.g. 200
            SamplingPeakProvider samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
            AveragePeakProvider averagePeakProvider = new AveragePeakProvider(4);
            StandardWaveFormRendererSettings setting = new StandardWaveFormRendererSettings();
            SolidBrush brush = new SolidBrush(Color.Green);
            

            //Setup
            setting.Width = width;
            setting.TopHeight = 75;
            setting.BottomHeight = 75;
            setting.BackgroundColor = Color.White;
            setting.PixelsPerPeak = 1;
            setting.TopPeakPen = new Pen(brush);
            setting.BottomPeakPen = new Pen(brush);
            setting.TopSpacerPen = new Pen(brush);

            //Render
            WaveFormRenderer renderer = new WaveFormRenderer();
            Image image = renderer.Render(reader, averagePeakProvider, setting);
            /* if (File.Exists(imagepath)) {
                 File.Delete(imagepath);
             }*/
            return image;
        }

        PictureBox DragTarget;
        bool Dragging;
        int Drag_Offset = 0;
        SampleEntry TargetSample;
        private async Task<float> WhileDragging()
        {
            while (true)
            {
                await Task.Delay(100);
                if (Dragging)
                {
                    int x = PointToClient(new Point(MousePosition.X-WorkPanel.Bounds.X, MousePosition.Y)).X + Drag_Offset;
                    if (x < TargetSample.pb_Spectrogram.Bounds.X)
                        x = TargetSample.pb_Spectrogram.Bounds.X;
                    if (x > TargetSample.pb_Spectrogram.Bounds.X + TargetSample.pb_Spectrogram.Width)
                        x = TargetSample.pb_Spectrogram.Bounds.X + TargetSample.pb_Spectrogram.Width;
                    //Clamp the second to the right of the first
                    if ((TargetSample.Handle1.Bounds.X + TargetSample.Handle1.Bounds.Width >TargetSample.Handle2.Bounds.X) && DragTarget == TargetSample.Handle1)
                    {
                        TargetSample.Handle2.SetBounds(TargetSample.Handle1.Bounds.X + TargetSample.Handle1.Bounds.Width + 1, TargetSample.Handle2.Bounds.Y, TargetSample.Handle2.Bounds.Width, TargetSample.Handle2.Bounds.Height);
                    }
                    //Clamp the second to 
                    if ((TargetSample.Handle2.Bounds.X < TargetSample.Handle1.Bounds.X) && DragTarget == TargetSample.Handle2)
                    {
                        TargetSample.Handle1.SetBounds(TargetSample.Handle2.Bounds.X - TargetSample.Handle1.Bounds.Width -1, TargetSample.Handle1.Bounds.Y, TargetSample.Handle1.Bounds.Width, TargetSample.Handle1.Bounds.Height);
                    }
                    DragTarget.SetBounds(x, DragTarget.Bounds.Y, DragTarget.Bounds.Width, DragTarget.Bounds.Height);
                    //Highlight
                    TargetSample.Highlight.SetBounds(TargetSample.Handle1.Bounds.X-TargetSample.pb_Spectrogram.Bounds.X, TargetSample.Highlight.Bounds.Y, TargetSample.Handle2.Bounds.X - TargetSample.Handle1.Bounds.X, TargetSample.Highlight.Height);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            PictureBox pb = new PictureBox();
            pb.MouseDown += Pb_MouseDown;
            pb.MouseUp += Pb_MouseUp;
            pb.BackColor = Color.Red;
            Controls.Add(pb);
            */
            float mOver = (float)trackBar1.Value / 1000;
            if (Directory.GetFiles("D:/VS Projects/AudioTest/AudioTest/Audio/Main Cut Segments").Count() > 0||Directory.GetFiles("D:\\VS Projects\\AudioTest\\AudioTest\\Audio/Work Segments").Count() > 0 || Directory.GetFiles("D:\\VS Projects\\AudioTest\\AudioTest\\Audio/Base Segments").Count() > 0)
            {
                DialogResult dr = MessageBox.Show("Files detected in directories, and will be deleted. Continue?", "Confirm Deletion", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    foreach (string s in Directory.GetFiles("D:\\VS Projects\\AudioTest\\AudioTest\\Audio/Work Segments"))
                    {
                        File.Delete(s);
                    }
                    foreach (string s in Directory.GetFiles("D:\\VS Projects\\AudioTest\\AudioTest\\Audio/Base Segments"))
                    {
                        File.Delete(s);
                    }
                    foreach (string s in Directory.GetFiles("D:\\VS Projects\\AudioTest\\AudioTest\\Audio/Main Cut Segments"))
                    {
                        File.Delete(s);
                    }
                    Task.Run(() => ParseAudio(mOver));
                }
            }
            else
                Task.Run(() => ParseAudio(mOver));
        }

        private void Pb_MouseUp(object sender, MouseEventArgs e)
        {
            Dragging = false;
        }

        private void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            DragTarget = (PictureBox)sender;
            foreach (SampleEntry se in InstEntries)
            {
                if (DragTarget == se.Handle1)
                {
                    TargetSample = se;
                    break;
                }
                else if (DragTarget == se.Handle2)
                {
                    TargetSample = se;
                    break;
                }
            }
            Dragging = true;
        }

        List<SampleEntry> InstEntries = new List<SampleEntry>();
        private void button2_Click(object sender, EventArgs e)
        {
            //On press, loop through the work segments, and create entries for each
            //Task.Run(() => GeneratePanels());
            foreach (string s in Directory.EnumerateFiles(baseDir + "/Work Segments"))
            {
                string n = s.Replace(baseDir + "/Work Segments\\", "");
                if (n.StartsWith("Part"))
                    continue;
                SampleEntry entry = new SampleEntry();
                Panel p = CreateSampleTemplate(entry, s);
                InstEntries.Add(entry);
                p.SetBounds(0, 0, p.Width, p.Height);
            }
            PositionPanels();
        }
        //Load Data
        private void button5_Click(object sender, EventArgs e)
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<SampleEntryData>));
            FileStream fs = new FileStream(baseDir + "/Autosave.xml",FileMode.Open);
            List<SampleEntryData> toLoad = (List<SampleEntryData>)xs.Deserialize(fs);
            fs.Close();
            foreach (string s in Directory.EnumerateFiles(baseDir + "/Work Segments"))
            {
                string n = s.Replace(baseDir + "/Work Segments\\", "");
                if (n.StartsWith("Part"))
                    continue;
                SampleEntry entry = new SampleEntry();
                Panel p = CreateSampleTemplate(entry, s);
                entry.LoadData(toLoad.Find(x => x.Filename == entry.audio.FileName));
                InstEntries.Add(entry);
                p.SetBounds(0, 0, p.Width, p.Height);
            }
            PositionPanels();
        }

        public async void GeneratePanels()
        {
            foreach (string s in Directory.EnumerateFiles(baseDir + "/Work Segments"))
            {
                string n = s.Replace(baseDir + "/Work Segments\\", "");
                if (n.StartsWith("Part"))
                    continue;
                SampleEntry entry = new SampleEntry();
                Panel p = CreateSampleTemplate(entry, s);
                InstEntries.Add(entry);
                p.SetBounds(0, 0, p.Width, p.Height);
            }
            PositionPanels();
        }

        public void SavePanelData()
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<SampleEntryData>));
            List<SampleEntry> toProc = InstEntries.Where(x =>x.Processed).ToList();
            List<SampleEntryData> toSave = new List<SampleEntryData>();
            TextWriter tw = new StreamWriter(baseDir + "/Autosave.xml");
            foreach (SampleEntry se in toProc)
                toSave.Add(se.CreateData());
            xs.Serialize(tw, toSave);
            tw.Close();
        }

        SampleEntry ButtonTarget = new SampleEntry();
        private SampleEntry GetTargetSampleFromButton(Button b)
        {
            foreach (SampleEntry se in InstEntries)
            {
                if (se.Play == b
                    || se.PlayNoSegment == b
                    || se.PlaySegment == b
                    || se.LockIn == b
                    || se.Discard == b)
                    return se;
            }
            return null;
        }

        WaveOutEvent AudioPlayer = new WaveOutEvent();
        private void B_Play_Click(object sender,EventArgs e)
        {
            //Setup
            ButtonTarget = GetTargetSampleFromButton((Button)sender);

            //Reset and Play
            AudioPlayer.Stop();
            ButtonTarget.audio.Position = 0;
            AudioPlayer.Init(ButtonTarget.audio);
            AudioPlayer.Play();
        }

        private void B_PlaySeg_Click(object sender, EventArgs e)
        {
            //Setup
            ButtonTarget = GetTargetSampleFromButton((Button)sender);
            //Set StartTime and Duration
            double startMilli = (double)ButtonTarget.Handle1.Bounds.X - ButtonTarget.pb_Spectrogram.Bounds.X;
            double duration = (double)ButtonTarget.Handle2.Bounds.X - ButtonTarget.Handle1.Bounds.X;
            startMilli = startMilli / ButtonTarget.pb_Spectrogram.Width;
            duration = duration / ButtonTarget.pb_Spectrogram.Width;
            startMilli *= ButtonTarget.audio.TotalTime.TotalMilliseconds;
            duration *= ButtonTarget.audio.TotalTime.TotalMilliseconds;
            if (duration + startMilli > ButtonTarget.audio.TotalTime.TotalMilliseconds)
                duration = ButtonTarget.audio.TotalTime.TotalMilliseconds - startMilli;

            //Trim
            ButtonTarget.audio.Position = 0;
            OffsetSampleProvider osp = new OffsetSampleProvider(ButtonTarget.audio);
            osp.SkipOver = TimeSpan.FromMilliseconds(startMilli);
            osp.Take = TimeSpan.FromMilliseconds(duration);

            //Reset and Play
            AudioPlayer.Stop();
            AudioPlayer.Init(osp);
            AudioPlayer.Play();
        }

        private void B_PlayWithoutSeg_Click(object sender,EventArgs e)
        {
            //Setup 
            ButtonTarget = GetTargetSampleFromButton((Button)sender);
            //Set Start Tims and Durations
            double Seg1Start = 0;
            double Seg1Duration = (double)ButtonTarget.Handle1.Bounds.X - ButtonTarget.pb_Spectrogram.Bounds.X;
            Seg1Duration = Seg1Duration / ButtonTarget.pb_Spectrogram.Width;
            Seg1Duration *= ButtonTarget.audio.TotalTime.TotalMilliseconds;
            double Seg2Start = (double)ButtonTarget.Handle2.Bounds.X - ButtonTarget.pb_Spectrogram.Bounds.X;
            Seg2Start = Seg2Start / ButtonTarget.pb_Spectrogram.Width;
            Seg2Start *= ButtonTarget.audio.TotalTime.TotalMilliseconds;
            double Seg2Duration = ButtonTarget.audio.TotalTime.TotalMilliseconds - Seg2Start;

            //Trim
            ButtonTarget.audio.Position = 0;
            OffsetSampleProvider Seg1 = new OffsetSampleProvider(ButtonTarget.audio);
            OffsetSampleProvider Seg2 = new OffsetSampleProvider(new AudioFileReader(ButtonTarget.audio.FileName));
            Seg1.SkipOver = TimeSpan.FromMilliseconds(Seg1Start);
            Seg1.Take = TimeSpan.FromMilliseconds(Seg1Duration);
            Seg2.SkipOver = TimeSpan.FromMilliseconds(Seg2Start);
            Seg2.Take = TimeSpan.FromMilliseconds(Seg2Duration);

            ISampleProvider playlist;
            if (Seg1Duration <= 0)
            {
                playlist = Seg2;
            }
            else if (Seg2Duration <=0)
            {
                playlist = Seg1;
            }
            else if (Seg1Duration <= 0 && Seg2Duration <= 0)
            {
                return;
            }
            else
            {
                playlist = Seg1.FollowedBy(Seg2); 
            }
            AudioPlayer.Stop();
            AudioPlayer.Init(playlist);
            AudioPlayer.Play();
        }

        bool ShowGreen = true;
        bool ShowRed = false;
        int page = 0;
        int MaxPages = 0;
        int PanelsPerPage = 50;
        int ReviewQueueSize = 50;
        private void PositionPanels()
        {
            //Update Pages
            PageOverflow();
            int[] vertOffsets = new int[2];
            int spacing = 25;
            int WorkPanelsShown = 0;
            int StitchPanelsIndex = 0;
            int totalProcessed = 0;
            foreach (SampleEntry se in InstEntries)
            {
                //Check if in work or Stitch
                if (se.Processed)
                {
                    //Stitching panel
                    if (
                        (se.Changed && ShowGreen)
                        || (!se.Changed && ShowRed)
                        )
                    {
                        totalProcessed++;
                        if (StitchPanelsIndex >= (page * PanelsPerPage) && StitchPanelsIndex < (page * PanelsPerPage) + PanelsPerPage)
                        {
                            if (!StitchPanel.Controls.Contains(se.panel))
                                StitchPanel.Controls.Add(se.panel);
                            se.panel.Visible = true;
                            se.panel.SetBounds(0, vertOffsets[0], se.panel.Bounds.Width, se.panel.Bounds.Height);
                            vertOffsets[0] += se.panel.Height + spacing;
                            StitchPanelsIndex++;
                        }
                        else if (StitchPanelsIndex < page * PanelsPerPage)
                        {
                            StitchPanelsIndex++;
                            if (StitchPanel.Contains(se.panel))
                                StitchPanel.Controls.Remove(se.panel);
                        }
                        else if (StitchPanelsIndex > (page*PanelsPerPage)+PanelsPerPage)
                        {
                            if (StitchPanel.Contains(se.panel))
                                StitchPanel.Controls.Remove(se.panel);
                        }
                    }
                    else
                        StitchPanel.Controls.Remove(se.panel);
                }
                else
                {
                    //Work Panel
                    if (WorkPanelsShown < ReviewQueueSize)
                    {
                        if (!WorkPanel.Controls.Contains(se.panel))
                            WorkPanel.Controls.Add(se.panel);
                        se.panel.SetBounds(0, vertOffsets[1], se.panel.Bounds.Width, se.panel.Bounds.Height);
                        vertOffsets[1] += se.panel.Height + spacing;
                        WorkPanelsShown++;
                    }
                }
            }
            MaxPages = (int)Math.Ceiling((double)totalProcessed / PanelsPerPage);
            SavePanelData();
            label1.Text = "Remaining Samples:\n" + InstEntries.Where(x => !x.Processed).ToList().Count;
        }

        private void B_Discard_Click(object sender, EventArgs e)
        {
            //Setup 
            ButtonTarget = GetTargetSampleFromButton((Button)sender);
            ButtonTarget.LockIn.Enabled = false;
            ButtonTarget.Discard.Text = "Send Back";
            DiscardLockHandler(false);
        }

        private void B_LockIn_Click(object sender, EventArgs e)
        {
            //Setup 
            ButtonTarget = GetTargetSampleFromButton((Button)sender);
            ButtonTarget.Discard.Enabled = false;
            ButtonTarget.LockIn.Text = "Send Back";
            DiscardLockHandler(true);
        }

        private void DiscardLockHandler(bool Lock)
        {
            if (ButtonTarget.Processed)
            {
                //Move back to work
                StitchPanel.Controls.Remove(ButtonTarget.panel);
                WorkPanel.Controls.Add(ButtonTarget.panel);

                ButtonTarget.panel.BackColor = Color.LightGray;

                ButtonTarget.LockIn.Enabled = true;
                ButtonTarget.Discard.Enabled = true;

                ButtonTarget.LockIn.Text = "Lock In";
                ButtonTarget.Discard.Text = "Discard";

                ButtonTarget.Processed = false;
                ButtonTarget.Changed = false;
            }
            else
            {
                //Move to Stitching
                WorkPanel.Controls.Remove(ButtonTarget.panel);
                StitchPanel.Controls.Add(ButtonTarget.panel);

                //Update Data
                ButtonTarget.Processed = true;
                ButtonTarget.Changed = Lock;

                if (Lock)
                    ButtonTarget.panel.BackColor = Color.DarkGreen;
                else
                    ButtonTarget.panel.BackColor = Color.DarkRed;
            }
            PositionPanels();
        }

        private Panel CreateSampleTemplate(SampleEntry se, string fileName)
        {
            Panel p = new Panel();
            se.panel = p;
            p.SetBounds(0, 0, 600, 200);
            p.BackColor = Color.LightGray;
            //Add Items
            //AudioFile
            se.audio = new AudioFileReader(fileName);
            
            //Spectro
            se.Spectrogram = CreateSpectro(400, se.audio);
            se.audio = new AudioFileReader(fileName);
            PictureBox pb_Spec = new PictureBox();
            pb_Spec.Image = se.Spectrogram;
            se.pb_Spectrogram = pb_Spec;
            p.Controls.Add(pb_Spec);
            pb_Spec.SetBounds(25, 25, 400, 125);

            //Name
            Label nameLabel = new Label();
            nameLabel.BackColor = Color.LightGray;
            p.Controls.Add(nameLabel);
            nameLabel.Text = fileName;
            nameLabel.SetBounds(5, pb_Spec.Bounds.Y - 15, 500, 25);
            nameLabel.AutoSize = true;

            //Handles
            PictureBox pb_Handle1 = new PictureBox();
            se.Handle1 = pb_Handle1;
            pb_Handle1.BackColor = Color.Cyan;
            pb_Handle1.MouseDown += Pb_MouseDown;
            pb_Handle1.MouseUp += Pb_MouseUp;
            p.Controls.Add(pb_Handle1);
            pb_Handle1.SetBounds((p.Width / 2)-30, 25, 20, 40);
            pb_Handle1.BringToFront();
            PictureBox pb_Handle2 = new PictureBox();
            se.Handle2 = pb_Handle2;
            pb_Handle2.BackColor = Color.Red;
            pb_Handle2.MouseDown += Pb_MouseDown;
            pb_Handle2.MouseUp += Pb_MouseUp;
            p.Controls.Add(pb_Handle2);
            pb_Handle2.SetBounds((p.Width / 2)+30, 25, 20, 40);
            

            //Filter
            PictureBox hl = new PictureBox();
            se.Highlight = hl;
            hl.BackColor = Color.Transparent;
            p.Controls.Add(hl);
            hl.Parent = pb_Spec;
            hl.Image = Image.FromFile(baseDir + "/GoldFill.png");
            hl.SizeMode = PictureBoxSizeMode.StretchImage;
            hl.SetBounds(pb_Handle1.Bounds.X-pb_Spec.Bounds.X, 25, pb_Handle2.Bounds.X - pb_Handle1.Bounds.X, pb_Spec.Height);
            hl.BringToFront();
            pb_Handle2.BringToFront();
            pb_Handle1.BringToFront();

            //Button Data
            int b_Width = 100;
            int b_Height = 25;
            int b_Offset = 5;

            //Play Button
            Button b_Play = new Button();
            b_Play.BackColor = Color.LightGray;
            se.Play = b_Play;
            p.Controls.Add(b_Play);
            b_Play.SetBounds(pb_Spec.Bounds.X, 25 + pb_Spec.Height + b_Offset, b_Width, b_Height);
            b_Play.Text = "Play";
            b_Play.Click += B_Play_Click;

            //Play Selection
            Button b_PlaySelection = new Button();
            b_PlaySelection.BackColor = Color.LightGray;
            se.PlaySegment = b_PlaySelection;
            p.Controls.Add(b_PlaySelection);
            b_PlaySelection.SetBounds(b_Play.Bounds.X + b_Width + 15, 25 + pb_Spec.Height + b_Offset, b_Width, b_Height);
            b_PlaySelection.Text = "Play Selected";
            b_PlaySelection.Click += B_PlaySeg_Click;

            //Play Without
            Button b_PlayWithout = new Button();
            b_PlayWithout.BackColor = Color.LightGray;
            se.PlayNoSegment = b_PlayWithout;
            p.Controls.Add(b_PlayWithout);
            b_PlayWithout.SetBounds(b_PlaySelection.Bounds.X + b_Width + 15, 25 + pb_Spec.Height + b_Offset, b_Width, b_Height);
            b_PlayWithout.Text = "Play Without";
            b_PlayWithout.Click += B_PlayWithoutSeg_Click;

            //Lock in
            Button b_Lock = new Button();
            b_Lock.BackColor = Color.LightGray;
            se.LockIn = b_Lock;
            p.Controls.Add(b_Lock);
            b_Lock.SetBounds(pb_Spec.Bounds.X + pb_Spec.Width + b_Offset, pb_Spec.Bounds.Y + 25, b_Width, b_Height);
            b_Lock.Text = "Lock In";
            b_Lock.Click += B_LockIn_Click;

            //Discard
            Button b_Discard = new Button();
            b_Discard.BackColor = Color.LightGray;
            se.Discard = b_Discard;
            p.Controls.Add(b_Discard);
            b_Discard.SetBounds(pb_Spec.Bounds.X + pb_Spec.Width + b_Offset, b_Lock.Bounds.Y+b_Height+b_Offset, b_Width, b_Height);
            b_Discard.Text = "Discard";
            b_Discard.Click += B_Discard_Click;

            //Return
            return p;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = "Match Threshold: " + (float)trackBar1.Value / 10 + "%";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDiag = new OpenFileDialog();
            fileDiag.InitialDirectory = baseDir;
            fileDiag.RestoreDirectory = true;

            if (fileDiag.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            SelectedFile = fileDiag.FileName;
            UpdateFileInfo();
        }

        private void UpdateFileInfo()
        {
            AudioFileReader f = new AudioFileReader(SelectedFile);
            f.Position = 0;
            FileInfo.Text = "File Name: " + SelectedFile +
                "\nDuration: " + f.TotalTime.Seconds;
        }

        private void Stitch()
        {
            //Setup
            List<string> MasterMergeList = new List<string>();

            //Delete Temp
            foreach (string s in Directory.GetFiles("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/TempSegments\\"))
                File.Delete(s);

            //Parse Work Segments
            List<StitchWorkSegment> WorkSegList = new List<StitchWorkSegment>();
            foreach (SampleEntry se in InstEntries)
            {
                if (se.Processed)
                {
                    //Set initial File Name
                    string fName = se.audio.FileName.Replace("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Work Segments\\", "");
                    fName = fName.Replace(".mp4", "");

                    //Get Task Pipeline
                    int taskInd = 0;
                    if (fName.StartsWith("1"))
                        taskInd = 1;
                    else if (fName.StartsWith("2"))
                        taskInd = 2;
                    else if (fName.StartsWith("3"))
                        taskInd = 3;
                    else if (fName.StartsWith("4"))
                        taskInd = 4;
                    fName = fName.Substring(2);
                    //Get prev and next
                    int dash = fName.IndexOf("-");
                    int prev = int.Parse(fName.Substring(0, fName.Length - dash - 1));
                    int next = int.Parse(fName.Substring(dash + 1));
                    //Init stitch segment

                    /*
                    StitchWorkSegment sws = new StitchWorkSegment(prev, next, taskInd - 1);
                    //Setup Segment
                    if (se.Changed)
                    {
                        sws.GenerateClip(se);
                    }
                    else
                        sws.loc = se.audio.FileName;
                    WorkSegList.Add(sws);
                    */
                }
            }

            //Parse Base Segments
            List<StitchBaseSegment> BaseSegList = new List<StitchBaseSegment>();
            foreach (string s in Directory.EnumerateFiles("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Base Segments"))
            {
                string fName = s.Replace("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Base Segments\\", "");
                fName = fName.Replace(".mp4", "");
                int taskInd = 0;
                if (fName.StartsWith("1"))
                    taskInd = 1;
                else if (fName.StartsWith("2"))
                    taskInd = 2;
                else if (fName.StartsWith("3"))
                    taskInd = 3;
                else if (fName.StartsWith("4"))
                    taskInd = 4;
                fName = fName.Substring(2);
                int segID = int.Parse(fName);
                StitchBaseSegment sbs = new StitchBaseSegment();
                sbs.SegmentID = segID;
                sbs.taskindex = taskInd - 1;
                sbs.loc = s;
                BaseSegList.Add(sbs);
            }
            //Setup string lists
            List<string>[] TaskStringLists = new List<string>[4];
            TaskStringLists[0] = GenerateFileList(BaseSegList, WorkSegList, 0);
            TaskStringLists[1] = GenerateFileList(BaseSegList, WorkSegList, 1);
            TaskStringLists[2] = GenerateFileList(BaseSegList, WorkSegList, 2);
            TaskStringLists[3] = GenerateFileList(BaseSegList, WorkSegList, 3);


            bool debug = true;
            if (debug)
            {
                //Output
                string outFile1 = "C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Output1.mp4";
                string outFile2 = "C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Output2.mp4";
                string outFile3 = "C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Output3.mp4";
                string outFile4 = "C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Output4.mp4";
                File.Delete(outFile1);
                File.Delete(outFile2);
                File.Delete(outFile3);
                File.Delete(outFile4);
                FFMpeg.Join(outFile1, TaskStringLists[0].ToArray());
                FFMpeg.Join(outFile2, TaskStringLists[1].ToArray());
                FFMpeg.Join(outFile3, TaskStringLists[2].ToArray());
                FFMpeg.Join(outFile4, TaskStringLists[3].ToArray());
            }
            else
            {
                //Concat
                MasterMergeList.AddRange(TaskStringLists[0]);
                MasterMergeList.AddRange(TaskStringLists[1]);
                MasterMergeList.AddRange(TaskStringLists[2]);
                MasterMergeList.AddRange(TaskStringLists[3]);

                //Output
                string outFile = baseDir + "/Output.mp4";
                File.Delete(outFile);
                FFMpeg.Join(outFile, MasterMergeList.ToArray());
            }
        }

        private void NewStitch()
        {
            //Parse Work Segments
            File.Delete(baseDir + "/Output.mp4");
            List<StitchWorkSegment> WorkSegList = new List<StitchWorkSegment>();
            List<string> OutList = new List<string>();
            int outInd = 1;
            AudioFileReader afr = new AudioFileReader(SelectedFile);
            //Setup the Files
            foreach (SampleEntry se in InstEntries)
            {
                if (se.Processed && se.Changed)
                {
                    //Set initial File Name
                    string fName = se.audio.FileName.Replace(baseDir + "/Work Segments\\", "");
                    fName = fName.Replace(".mp4", "");

                    //Get the start and end milliseconds
                    int dash = fName.IndexOf("-");
                    int prev = 0;
                    if (fName.StartsWith("9103"))
                    {
                        int g = 0;
                    }
                    if (!fName.StartsWith("0-"))
                    {
                        string tParse = fName.Substring(0, dash);
                        prev = int.Parse(tParse);
                    }
                    int next = int.Parse(fName.Substring(dash + 1));
                    //Init stitch segment
                    StitchWorkSegment sws = new StitchWorkSegment(prev, next, 0, se, afr.TotalTime.TotalMilliseconds, prev);
                    sws.loc = se.audio.FileName;
                    WorkSegList.Add(sws);
                    //Add things to Debug List
                    OutList.Add(outInd + ":  " + fName);
                }
            }
            //Order the files by order of happening
            List<StitchWorkSegment> OrderedWorkSegs = new List<StitchWorkSegment>();
            while (WorkSegList.Count > 0)
            {
                int lowIndex = 0;
                foreach (StitchWorkSegment sws in WorkSegList)
                    //if (sws.seg1Duration <= WorkSegList[lowIndex].seg1Duration)
                    if (sws.prevSeg <= WorkSegList[lowIndex].prevSeg)
                        lowIndex = WorkSegList.IndexOf(sws);
                OrderedWorkSegs.Add(WorkSegList[lowIndex]);
                WorkSegList.RemoveAt(lowIndex);
            }

            //Update Debug
            List<string> OutList2 = new List<string>();
            int out2 = 1;
            foreach (StitchWorkSegment swDebug in OrderedWorkSegs)
            {
                OutList2.Add(out2 + ":  " + swDebug.prevSeg);
                out2++;
            }
            //Update the Ordered List startIndexes and Durations
            //Seg1Duration is from 0 to the start of the splice. seg2 duration is from the end of the splice to the end of the clip.
            //
            double newStartTime = 0;
            for (int i = 0;i<OrderedWorkSegs.Count;i++)
            {

                OrderedWorkSegs[i].seg1Start = newStartTime;
                OrderedWorkSegs[i].seg1Duration = OrderedWorkSegs[i].seg1Duration - newStartTime;
                if (i > 0)
                {

                }


                //Incra
                newStartTime = OrderedWorkSegs[i].seg2Start;
            }
            //Process the files, and update the durations as needed.
            object[] mVars = new object[2];
            mVars[0] = label7;
            List<string> files = new List<string>();
            foreach (StitchWorkSegment sws in OrderedWorkSegs)
            {
                mVars[1] = "Stitching " + OrderedWorkSegs.IndexOf(sws) + "/" + OrderedWorkSegs.Count;
                label7.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
                bool finalStitch = false;
                if (OrderedWorkSegs.Last() == sws)
                    finalStitch = true;
                files.AddRange(sws.GenerateClip2(sws.entry, SelectedFile,finalStitch));
            }
            //Stitch Together
            //FFMpeg.Join(baseDir + "/Output.mp4", files.ToArray());
            //New Method
            string[] vids = new string[2];
            vids[0] = files[0];
            vids[1] = files[1];
            FFMpeg.Join(baseDir + "/Output.mp4", vids);
            for (int i = 2;i<files.Count;i++)
            {
                vids[0] = baseDir + "/Output.mp4";
                vids[1] = files[i];
                FFMpeg.Join(baseDir + "/Output.mp4", vids);
            }

            //Update Label
            mVars[1] = "Stitch Complete";
            label7.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
        }


        private void NewStitch2()
        {
            //Parse Work Segments
            File.Delete(baseDir + "/Output.mp4");
            List<StitchWorkSegment> WorkSegList = new List<StitchWorkSegment>();
            List<string> OutList = new List<string>();
            int outInd = 1;
            AudioFileReader afr = new AudioFileReader(SelectedFile);
            //Setup the Files
            foreach (SampleEntry se in InstEntries)
            {
                if (se.Processed && se.Changed)
                {
                    //Set initial File Name
                    string fName = se.audio.FileName.Replace("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Work Segments\\", "");
                    fName = fName.Replace(".mp4", "");

                    //Get the start and end milliseconds
                    int dash = fName.IndexOf("-");
                    int prev = 0;
                    if (fName.StartsWith("9103"))
                    {
                        //debug purposes
                        int g = 0;
                    }
                    if (!fName.StartsWith("0-"))
                    {
                        string tParse = fName.Substring(0, dash);
                        prev = int.Parse(tParse);
                    }
                    int next = int.Parse(fName.Substring(dash + 1));
                    //Init stitch segment
                    StitchWorkSegment sws = new StitchWorkSegment(prev, next, 0, se, afr.TotalTime.TotalMilliseconds, prev);
                    sws.loc = se.audio.FileName;
                    WorkSegList.Add(sws);
                    //Add things to Debug List
                    OutList.Add(outInd + ":  " + fName);
                }
            }
            //Order the files by order of happening
            List<StitchWorkSegment> OrderedWorkSegs = new List<StitchWorkSegment>();
            while (WorkSegList.Count > 0)
            {
                int lowIndex = 0;
                foreach (StitchWorkSegment sws in WorkSegList)
                    //if (sws.seg1Duration <= WorkSegList[lowIndex].seg1Duration)
                    if (sws.prevSeg <= WorkSegList[lowIndex].prevSeg)
                        lowIndex = WorkSegList.IndexOf(sws);
                OrderedWorkSegs.Add(WorkSegList[lowIndex]);
                WorkSegList.RemoveAt(lowIndex);
            }

            //Update Debug
            List<string> OutList2 = new List<string>();
            int out2 = 1;
            foreach (StitchWorkSegment swDebug in OrderedWorkSegs)
            {
                OutList2.Add(out2 + ":  " + swDebug.prevSeg);
                out2++;
            }
            //Update the Ordered List startIndexes and Durations
            //Seg1Duration is from 0 to the start of the splice. seg2 duration is from the end of the splice to the end of the clip.
            //
            double newStartTime = 0;
            for (int i = 0; i < OrderedWorkSegs.Count; i++)
            {

                OrderedWorkSegs[i].seg1Start = newStartTime;
                OrderedWorkSegs[i].seg1Duration = OrderedWorkSegs[i].seg1Duration - newStartTime;
                if (i > 0)
                {

                }


                //Incra
                newStartTime = OrderedWorkSegs[i].seg2Start;
            }
            //Process the files, and update the durations as needed.
            object[] mVars = new object[2];
            mVars[0] = label7;
            List<string> files = new List<string>();
            foreach (StitchWorkSegment sws in OrderedWorkSegs)
            {
                mVars[1] = "Stitching " + OrderedWorkSegs.IndexOf(sws) + "/" + OrderedWorkSegs.Count;
                label7.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
                bool finalStitch = false;
                if (OrderedWorkSegs.Last() == sws)
                    finalStitch = true;
                files.AddRange(sws.GenerateClip2(sws.entry, SelectedFile, finalStitch));
            }
            //Stitch Together
            //FFMpeg.Join("C:/Users/jedim/source/repos/AudioTest/AudioTest/Audio/Output.mp4", files.ToArray());
            //New Method
            string[] vids = new string[2];
            vids[0] = files[0];
            vids[1] = files[1];
            FFMpeg.Join(baseDir + "/Output.mp4", vids);
            for (int i = 2; i < files.Count; i++)
            {
                vids[0] = baseDir + "/Output.mp4";
                vids[1] = files[i];
                FFMpeg.Join(baseDir + "/Output.mp4", vids);
            }

            //Update Label
            mVars[1] = "Stitch Complete";
            label7.BeginInvoke(new TextUpdateDelegate(DelegateTextUpdate), mVars);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (SelectedFile == "")
            {
                System.Windows.Forms.MessageBox.Show("No file selected. Please do so from the input tab");
            }
            else
            {
                label7.Text = "Stitching...";
                ClearTemp();
                Task.Run(() => NewStitch());
                label7.Text = "Stitch Complete";
            }
        }

        private void ClearTemp()
        {
            foreach (string s in Directory.EnumerateFiles(baseDir + "/TempSegments"))
            {
                File.Delete(s);
            }
        }

        public List<string> GenerateFileList(List<StitchBaseSegment> baseSegs, List<StitchWorkSegment> workSegs, int targetTaskIndex)
        {
            int tail = 0;
            List<string> returnList = new List<string>();
            while (tail < baseSegs.Count)
            {
                //Loop through BaseSegs for tail
                foreach (StitchBaseSegment sbs in baseSegs)
                {
                    if (sbs.taskindex == targetTaskIndex)
                        if (sbs.SegmentID == tail)
                        {
                            returnList.Add(sbs.loc);
                            break;
                        }
                }
                //Find any Work segments that might follow
                foreach (StitchWorkSegment sws in workSegs)
                {
                    if (sws.taskindex == targetTaskIndex)
                        if (sws.prevSeg == tail)
                            if (!sws.skip)
                                returnList.Add(sws.loc);
                }
                //incra
                tail++;
            }
            return returnList;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //Green Toggle
            if (checkBox2.Checked)
            {
                ShowGreen = true;
            }
            else
                ShowGreen = false;
            PositionPanels();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //Red Toggle
            if (checkBox1.Checked)
            {
                ShowRed = true;
            }
            else
                ShowRed = false;
            PositionPanels();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            page--;
            PageOverflow();
            PositionPanels();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            page++;
            PageOverflow();
            PositionPanels();
        }

        private void PageOverflow()
        {
            if (page > MaxPages)
                page = 0;
            if (page < 0)
                page = MaxPages;
            ViewLabel.Text = "Viewing " + page * PanelsPerPage + " - " + ((page * PanelsPerPage) + PanelsPerPage) + "/" + MaxPages * PanelsPerPage;
        }

        
    }



    public class StitchSegment
    {
        public int taskindex;
        public string loc;
        public int vidWidth = 1620;
        public int vidHeight = 1080;
    }

    public class StitchWorkSegment : StitchSegment
    {
        public int prevSeg = 0;
        public int nextSeg = 0;
        public bool skip;
        public SampleEntry entry;

        public double seg1Start;
        public double seg2Start;
        public double seg1Duration;
        public double seg2Duration;

        public void ReduceTime(double time, double Marker)
        {
            if (seg1Duration <= Marker)
                return;
            seg1Duration -= time;
            seg2Start -= time;
            seg2Duration += time;
        }

        public StitchWorkSegment(int prev, int next, int TaskIndex, SampleEntry se, double totalMilli, double GlobalScale = 0)
        {
            entry = se;
            prevSeg = prev;
            nextSeg = next;
            taskindex = TaskIndex;
            loc = Form1.baseDir + "/TempSegments/"+taskindex+ "-" + prev + "-" + nextSeg + ".mp4";

            //Local Durations
            double Seg1Start = 0;
            double Seg1Duration = (double)se.Handle1.Bounds.X - se.pb_Spectrogram.Bounds.X;
            Seg1Duration = Seg1Duration / se.pb_Spectrogram.Width;
            Seg1Duration *= se.audio.TotalTime.TotalMilliseconds;
            Seg1Duration = Math.Floor(Seg1Duration);
            double Seg2Start = (double)se.Handle2.Bounds.X - se.pb_Spectrogram.Bounds.X;
            Seg2Start = Seg2Start / se.pb_Spectrogram.Width;
            Seg2Start *= se.audio.TotalTime.TotalMilliseconds;
            Seg2Start = Math.Ceiling(Seg2Start);

            //Adjust to global scale
            Seg1Duration += GlobalScale;
            Seg2Start += GlobalScale;
            double Seg2Duration = totalMilli - Seg2Start;

            //Update
            seg1Start = Seg1Start;
            seg2Start = Seg2Start;
            seg1Duration = Seg1Duration;
            seg2Duration = Seg2Duration;
        }

        public List<string> GenerateClip2(SampleEntry se, string fileName, bool Final = false)
        {
            List<string> retList = new List<string>();
            MediaFile sourceMedia = new MediaFile(fileName);
            if (File.Exists(Form1.baseDir + "/Output.mp4"))
                sourceMedia = new MediaFile(Form1.baseDir + "/Output.mp4");
            using (Engine e = new Engine())
            {
                ConversionOptions options = new ConversionOptions();
                e.GetMetadata(sourceMedia);
                string t1Name = Form1.baseDir + "/TempSegments/t-" +seg1Start +".mp4";
                string t2Name = Form1.baseDir + "/TempSegments/t-" +seg2Start +".mp4";
                File.Delete(t1Name);
                File.Delete(t2Name);
                MediaFile t1 = new MediaFile(t1Name);
                MediaFile t2 = new MediaFile(t2Name);
                //If file is too short, just skip
                if (seg1Duration <= 0)
                    return retList;
                //Cut the previous segment. if the final one, also cut the last segment
                options.CutMedia(TimeSpan.FromMilliseconds(seg1Start), TimeSpan.FromMilliseconds(seg1Duration));
                e.Convert(sourceMedia, t1, options);
                retList.Add(t1Name);
                if (Final)
                {
                    options.CutMedia(TimeSpan.FromMilliseconds(seg2Start), TimeSpan.FromMilliseconds(seg2Duration));
                    e.Convert(sourceMedia, t2, options);
                    retList.Add(t2Name);
                }

                //FFMpeg.Join(Form1.baseDir + "/Output.mp4", t1.Filename, t2.Filename);
            }
            

            return retList;
        }

        public void GenerateClip(SampleEntry se)
        {
            //Local Durations
            double Seg1Start = 0;
            double Seg1Duration = (double)se.Handle1.Bounds.X - se.pb_Spectrogram.Bounds.X;
            Seg1Duration = Seg1Duration / se.pb_Spectrogram.Width;
            Seg1Duration *= se.audio.TotalTime.TotalMilliseconds;
            double Seg2Start = (double)se.Handle2.Bounds.X - se.pb_Spectrogram.Bounds.X;
            Seg2Start = Seg2Start / se.pb_Spectrogram.Width;
            Seg2Start *= se.audio.TotalTime.TotalMilliseconds;
            double Seg2Duration = se.audio.TotalTime.TotalMilliseconds - Seg2Start;
            if (Seg1Duration <= 0 && Seg2Duration <= 0)
            {
                skip = true;
                return;
            }

            //
            using (Engine e = new Engine())
            {
                ConversionOptions options = new ConversionOptions();
                MediaFile source = new MediaFile(se.audio.FileName);
                e.GetMetadata(source);
                File.Delete(Form1.baseDir + "/TempSegments/t1.mp4");
                File.Delete(Form1.baseDir + "/TempSegments/t2.mp4");
                MediaFile t1 = new MediaFile(Form1.baseDir + "/TempSegments/t1.mp4");
                MediaFile t2 = new MediaFile(Form1.baseDir + "/TempSegments/t2.mp4");
                bool t1Skipped = true;
                bool t2Skipped = true;
                if (Seg1Duration > 0)
                {
                    t1Skipped = false;
                    options.CutMedia(TimeSpan.FromMilliseconds(Seg1Start), TimeSpan.FromMilliseconds(Seg1Duration));
                    e.Convert(source, t1, options);
                }
                if (Seg2Duration > 0)
                {
                    t2Skipped = false;
                    options.CutMedia(TimeSpan.FromMilliseconds(Seg2Start), TimeSpan.FromMilliseconds(Seg2Duration));
                    e.Convert(source, t2, options);
                }

                //Merge and save
                //If either is skipped, then just save the other part
                MediaFile locFile = new MediaFile(loc);
                if (t1Skipped)
                {
                    options.CutMedia(TimeSpan.FromMilliseconds(Seg2Start), TimeSpan.FromMilliseconds(Seg2Duration));
                    e.Convert(source, locFile, options);
                }
                else if (t2Skipped)
                {
                    options.CutMedia(TimeSpan.FromMilliseconds(Seg1Start), TimeSpan.FromMilliseconds(Seg1Duration));
                    e.Convert(source, locFile, options);
                }
                else
                {
                    //Merge the two files
                    FFMpeg.Join(loc, t1.Filename, t2.Filename);
                }
            }
        }
    }

    public class StitchBaseSegment : StitchSegment
    {
        public int SegmentID;
    }

    [System.Serializable]
    public class SampleEntryData
    {
        public int H1x;
        public int H2x;
        public string Filename;
        public bool Processed = false;
        public bool Changed = false;
    }

    public class SampleEntry
    {
        public Panel panel;
        public PictureBox Handle1;
        public PictureBox Handle2;
        public PictureBox Highlight;
        public PictureBox pb_Spectrogram;
        public Image Spectrogram;
        public Button LockIn;
        public Button Discard;
        public Button Play;
        public Button PlaySegment;
        public Button PlayNoSegment;
        public AudioFileReader audio;
        public bool Processed = false;
        public bool Changed = false;

        public void LoadData(SampleEntryData sed)
        {
            if (sed is null)
                return;
            Handle1.SetBounds(sed.H1x, Handle1.Bounds.Y, Handle1.Width, Handle1.Height);
            Handle2.SetBounds(sed.H2x, Handle2.Bounds.Y, Handle2.Width, Handle2.Height);
            Highlight.SetBounds(Handle1.Bounds.X - pb_Spectrogram.Bounds.X, Highlight.Bounds.Y, Handle2.Bounds.X - Handle1.Bounds.X, Highlight.Height);
            Processed = sed.Processed;
            Changed = sed.Changed;
            if (Changed)
            {
                panel.BackColor = Color.DarkGreen;
                Discard.Enabled = false;
                LockIn.Text = "Send Back";
            }
            else
            {
                panel.BackColor = Color.DarkRed;
                LockIn.Enabled = false;
                Discard.Text = "Send Back";
            }
        }

        public SampleEntryData CreateData()
        {
            SampleEntryData sed = new SampleEntryData();
            sed.Changed = Changed;
            sed.Processed = Processed;
            sed.Filename = audio.FileName;
            sed.H1x = Handle1.Bounds.X;
            sed.H2x = Handle2.Bounds.X;
            return sed;
        }
    }

    class SampleData
    {
        public Image Spectrogram;
        public Bitmap bitmap;
        public float bWidth;
        public float bHeight;
        public Color[,] ColorArray;
        public List<List<bool>> MatchArray = new List<List<bool>>();
        public int MatchCount = 0;
        public long Length;

        public void GenerateColorArray()
        {
            Color[,] tempArray = new Color[bitmap.Width, bitmap.Height];
            for (int i = 0;i<bitmap.Width;i++)
                for (int j = 0;j<bitmap.Height;j++)
                {
                    tempArray[i, j] = bitmap.GetPixel(i, j);
                }
            ColorArray = tempArray;
        }

        public int GetNextMatch(Bitmap fullPic)
        {
            if (MatchArray.Count<=0)
                InitMatch(fullPic);
            for (int i = 0; i<MatchArray.Count;i++)
            {
                bool b = MatchArray[i][0];
                MatchArray[i].RemoveAt(0);
                if (b)
                    MatchCount--;
                bool m = fullPic.GetPixel((int)bWidth-1, i) == ColorArray[(int)bWidth-1, i];
                if (m)
                    MatchCount++;
                MatchArray[i].Add(m);
            }
            return MatchCount;
        }

        public void InitMatch(Bitmap fullPic)
        {
            //Inversed i and j to go row by row, instead of column by column
            MatchArray = new List<List<bool>>();
            MatchCount = 0;
            for (int j = 0; j < bitmap.Height; j++)
            {
                List<bool> tList = new List<bool>();
                for (int i = 0; i < bitmap.Width; i++)
                {
                    bool b = fullPic.GetPixel(i, j) == ColorArray[i, j];
                    if (b)
                        MatchCount++;
                    tList.Add(b);
                }
                //Add the last row
                MatchArray.Add(tList);
            }
        }
    }
}
