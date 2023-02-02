using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace UiPathTeam.VideoProcessing.Activities
{
    [DisplayName("Extract Frames - JPEG"), Description("Extract Frames from an MP4 video file and save them as JPEG images")]
    public class ExtractFramesJPEG : NativeActivity
    {
        #region Properties

        [Category("Input"), Description("Fully rooted file path to the input video file (.mp4)")]
        [RequiredArgument]
        public InArgument<String> MP4File { get; set; }

        [Category("Input"), Description("Fully rooted output folder path to save the image frames into")]
        [RequiredArgument]
        public InArgument<String> OutputFolder { get; set; }

        [Category("Input"), Description("Fully rooted folder Path to the installed FFMPEG Library.  https://ffmpeg.org  e.g. \"C:\\ffmpeg\"")]
        [RequiredArgument]
        public InArgument<String> FFMpegFolder { get; set; }

        [Category("Input"), Description("The time at which to start the extraction.  Default: 00:00")]
        public InArgument<TimeSpan> SegmentStart { get; set; } = new InArgument<TimeSpan>(TimeSpan.Zero);

        [Category("Input"), Description("The duration of the extraction.  Leave blank for the entire duration of the video file.")]
        public InArgument<TimeSpan> SegmentDuration { get; set; }

        [Category("Input"), Description("The maximum number of frames that will be saved to the folder.  Set to 0 for unlimited.  Default: 1000")]
        public InArgument<Int32> MaxFrameCount { get; set; } = new InArgument<Int32>(1000);

        [Category("Input"), Description("The frequency at which consecutive images are captured (ie Frame Rate).  Default: 3")]
        public InArgument<Int32> FramesPerSecond { get; set; } = new InArgument<Int32>(3);

        private int MaxFPS = 50;

        private string TotalDuration = "0";

        #endregion

        #region NativeActivity

        protected override void Execute(NativeActivityContext context)
        {
            ValidateParameters(context);

            Logger.Instance.Info("Frame Extraction Process Started");
            Process process = new Process();

            try
            {
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = FFMpegFolder.Get(context);
                string td = TotalDuration.Equals("0") ? "" : " -t " + TotalDuration;
                process.StartInfo.Arguments = " -ss " + SegmentStart.Get(context) + td + " -i " + "\"" + MP4File.Get(context) + "\"" + " -r " + FramesPerSecond.Get(context).ToString() + " " + "\"" + Path.Combine(OutputFolder.Get(context), "image%04d.jpeg") + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                Logger.Instance.Trace("Running the following command: " + process.StartInfo.FileName + process.StartInfo.Arguments);
                process.Start();
                process.WaitForExit();

                Logger.Instance.Trace("Total processing time in milliseconds: " + process.TotalProcessorTime.TotalMilliseconds.ToString());
                Logger.Instance.Info("Frame Extraction Process Completed");
            }
            catch (Exception ex)
            {
                throw new Exception(process.StandardError.ToString());
            }
            finally
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

        #region HelperMethods

        private void ValidateParameters(NativeActivityContext context)
        {
            Logger.Instance.Trace("MP4File: " + MP4File.Get(context));
            Logger.Instance.Trace("OutputFolder: " + OutputFolder.Get(context));

            if (!File.Exists(MP4File.Get(context)))
            {
                throw new Exception("Input File Not Found: " + MP4File.Get(context));
            }

            if (!String.Equals(Path.GetExtension(MP4File.Get(context)), ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Input File extension must be .mp4: " + Path.GetFileName(MP4File.Get(context)));
            }

            if (!Path.IsPathRooted(OutputFolder.Get(context)))
            {
                throw new Exception("Output Folder must be Fully Rooted: " + OutputFolder.Get(context));
            }

            if (!Directory.Exists(OutputFolder.Get(context)))
            {
                throw new Exception("Output Folder does not exist: " + OutputFolder.Get(context));
            }

            if (!Directory.Exists(FFMpegFolder.Get(context)))
            {
                throw new Exception("FFMpeg Folder not found: " + FFMpegFolder.Get(context));
            }

            string FFMpegBin = Path.Combine(FFMpegFolder.Get(context), "bin", "ffmpeg.exe");
            string FFMpegRoot = Path.Combine(FFMpegFolder.Get(context), "ffmpeg.exe");
            if (!File.Exists(FFMpegBin))
            {
                if (!File.Exists(FFMpegRoot))
                {
                    throw new Exception("FFMpeg Library not found: " + FFMpegBin);
                }
                else
                {
                    FFMpegFolder.Set(context, FFMpegRoot);
                }
            }
            else
            {
                FFMpegFolder.Set(context, FFMpegBin);
            }

            if (SegmentStart.Get(context) == null || SegmentStart.Get(context).CompareTo(TimeSpan.Zero) < 0)
            {
                Logger.Instance.Warning("Invalid Segment Start.  Defaulting to 00:00:00");
                SegmentStart.Set(context, TimeSpan.Zero);
            }
            
            if (SegmentDuration.Get(context).CompareTo(TimeSpan.Zero) < 0)
            {
                Logger.Instance.Warning("Invalid Segment Duration.  Defaulting to 00:00:00");
                SegmentDuration.Set(context, TimeSpan.Zero);
            }

            //Ensure Frames per Second is within acceptable range

            if (FramesPerSecond.Get(context) < 1)
            {
                Logger.Instance.Warning("(" + FramesPerSecond.Get(context).ToString() + ")" + " FPS is too low. Adjusted to 1 FPS");
                FramesPerSecond.Set(context, 1);
            }
            else if (FramesPerSecond.Get(context) > MaxFPS)
            {
                Logger.Instance.Warning("(" + FramesPerSecond.Get(context).ToString() + ")" + " FPS is too high. Adjusted to " + MaxFPS.ToString() + " FPS");
                FramesPerSecond.Set(context, MaxFPS);
            }

            // Ensure that the number of frames that will be produced is within user-defined limits
            int count = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(SegmentDuration.Get(context).TotalSeconds * FramesPerSecond.Get(context))));
            if (SegmentDuration.Get(context).CompareTo(TimeSpan.Zero) >= 0)
            {
                TotalDuration = Convert.ToInt32(SegmentDuration.Get(context).TotalSeconds).ToString();

                Logger.Instance.Trace("Frame Rate: " + FramesPerSecond.Get(context).ToString() + " fps");
                Logger.Instance.Trace("Duration: " + TotalDuration + " seconds");
                Logger.Instance.Trace("Estimated Frame Count: " + count.ToString() + " Frames");

                if (MaxFrameCount.Get(context) > 0 && count > MaxFrameCount.Get(context))
                {
                    throw new Exception("The number of frames that will be produced +(" + count.ToString() + ")+ exceeds your limit of (" + MaxFrameCount.Get(context).ToString() + ").  Try reducing the FramesPerSecond or reducing the SegmentDuration.  Otherwise, you can remove the SegmentDuration Argument.   To disable this limit, set MaxFrameCount to 0");
                }
            }
        }

        #endregion

    }
}