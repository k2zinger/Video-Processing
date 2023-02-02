using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace UiPathTeam.VideoProcessing.Activities
{
    [DisplayName("Extract Audio - MP3"), Description("Extract Audio from an MP4 video file and it as an MP3 file")]
    public class ExtractAudioMP3 : NativeActivity
    {
        #region Properties

        [Category("Input"), Description("Fully rooted file path to the input video file (.mp4)")]
        [RequiredArgument]
        public InArgument<String> MP4File { get; set; }

        [Category("Input"), Description("Fully rooted file path to output audio file (.mp3)")]
        [RequiredArgument]
        public InArgument<String> MP3File { get; set; }

        [Category("Input"), Description("Fully rooted folder Path to the installed FFMPEG Library.  https://ffmpeg.org  e.g. \"C:\\ffmpeg\"")]
        [RequiredArgument]
        public InArgument<String> FFMpegFolder { get; set; }

        #endregion

        #region NativeActivity

        protected override void Execute(NativeActivityContext context)
        {
            ValidateParameters(context);

            ExecuteJob(context);
        }

        #endregion

        #region HelperMethods

        public void ValidateParameters(NativeActivityContext context)
        {
            Logger.Instance.Trace("MP4File: " + MP4File.Get(context));
            Logger.Instance.Trace("MP3File: " + MP3File.Get(context));

            if (!File.Exists(MP4File.Get(context)))
            {
                throw new Exception("Input File Not Found: " + MP4File.Get(context));
            }

            if (!String.Equals(Path.GetExtension(MP4File.Get(context)), ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Input File extension must be .mp4: " + Path.GetFileName(MP4File.Get(context)));
            }

            if (!Path.IsPathRooted(MP3File.Get(context)))
            {
                throw new Exception("Output Filepath must be Fully Rooted: " + MP3File.Get(context));
            }

            if (!Directory.Exists(Path.GetDirectoryName(MP3File.Get(context))))
            {
                throw new Exception("Output Filepath does not exist: " + MP3File.Get(context));
            }

            if (File.Exists(MP3File.Get(context)))
            {
                throw new Exception("Output File already exists: " + MP3File.Get(context));
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
        }

        public void ExecuteJob(NativeActivityContext context)
        {
            Logger.Instance.Info("Audio Extraction Process Started");
            Process process = new Process();

            try
            {
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = FFMpegFolder.Get(context);
                process.StartInfo.Arguments = " -i " + "\"" + MP4File.Get(context) + "\"" + " -q:a 0 -map a -y " + "\"" + MP3File.Get(context) + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                Logger.Instance.Trace("Running the following command: " + process.StartInfo.FileName + process.StartInfo.Arguments);
                process.Start();
                process.WaitForExit();

                Logger.Instance.Trace("Total processing time in milliseconds: " + process.TotalProcessorTime.TotalMilliseconds.ToString());
                Logger.Instance.Info("Audio Extraction Process Completed");
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

    }
}