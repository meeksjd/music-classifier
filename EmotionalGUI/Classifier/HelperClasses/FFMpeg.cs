﻿using System.Linq;
using System.IO;

namespace Classifier
{
    public static class FFMpeg
    {
        /// <summary>
        /// Returns a list of fully qualified paths to the converted audio files
        /// </summary>
        /// <param name="files">list of .mp3 files</param>
        /// <returns>List of .wav files</returns>
        public static string[] ffmpegConversion(string[] files)
        {
            //bextract requires .wav files, but user will give .mp3 files.
            //ffmpeg will make temporary .wav files.
            string[] tempfiles = new string[files.Length];

            int i = 0;
            foreach (string file in files)
            {
                //First create temporary file
                string newfile = ExecutableInformation.getTmpPath() + "/" + Path.GetFileNameWithoutExtension(file);
                while (files.Contains(newfile + ".wav"))
                {
                    newfile += "_dup";
                }
                newfile += ".wav";
                tempfiles[i] = Path.GetFullPath(newfile);

                //Run FFMPEG
                string ffmpegArgs = "-loglevel quiet -y -i " + file + " " + newfile;

                System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
                ffmpeg.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.RedirectStandardOutput = true;   // Redirect so we can read the standard output
                ffmpeg.StartInfo.Arguments = ffmpegArgs;
                ffmpeg.StartInfo.FileName = ExecutableInformation.getFFMPegPath();

                ffmpeg.Start();
                ffmpeg.WaitForExit();
            }
            return tempfiles;
        }
    }
}
