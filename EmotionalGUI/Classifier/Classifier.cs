﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Accord;

namespace Classifier
{
    public class Classifier
    {

        class SongData
        {
            string filename = "";
            List<List<Double>> features =  new List<List<Double>>();

            public void AddFeatureList(List<Double> feature_list)
            {
                features.Add(feature_list);
            }

            public void SetFilename(string in_filename)
            {
                filename = in_filename;
            }
        }

        protected string COMMON_UTILITIES_DIRECTORY;
        protected string BEXTRACT_FILENAME;
        protected string FFMPEG_FILENAME;
        protected string TEMP_DIRECTORY;
        protected string WINDOW_FS;
        protected int NUM_BEXTRACT_COLUMNS = 62;

        public Classifier()
        {
            COMMON_UTILITIES_DIRECTORY = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../../common_utilities");
            BEXTRACT_FILENAME = Path.GetFullPath(COMMON_UTILITIES_DIRECTORY + "/bextract.exe");
            FFMPEG_FILENAME = Path.GetFullPath(COMMON_UTILITIES_DIRECTORY + "/ffmpeg.exe");
            TEMP_DIRECTORY = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../tmp");

            WINDOW_FS = (1 << 21).ToString();
        }

        public string classifySongs(string[] in_songs)
        {

            //Create temporary directory
            if (!Directory.Exists(TEMP_DIRECTORY))
            {
                Directory.CreateDirectory(TEMP_DIRECTORY);
            }

            //Do FFMPEG conversion
            string[] tempfiles = ffmpegConversion(in_songs);

            //Create mkcollection
            string mkcollection = TEMP_DIRECTORY + "/music.mk";
            File.WriteAllLines(mkcollection, tempfiles);

            //Run bextract
            runBextract(mkcollection);

            //Parse bextract data
            parseArff(TEMP_DIRECTORY + "/MARSYAS_EMPTY");
            
            return "";
        }

        protected string[] ffmpegConversion(string[] files)
        {
            //bextract requires .wav files, but user will give .mp3 files.
            //ffmpeg will make temporary .wav files.
            string[] tempfiles = new string[files.Length];  

            int i = 0;
            foreach (string file in files)
            {
                //First create temporary file
                string newfile = TEMP_DIRECTORY + "/" + Path.GetFileNameWithoutExtension(file);
                while (files.Contains(newfile + ".wav"))
                {
                    newfile += "_dup";
                }
                newfile += ".wav";
                tempfiles[i] = Path.GetFullPath(newfile);

                //Run FFMPEG
                string ffmpegArgs =  "-loglevel quiet -y -i " + file + " " + newfile;

                System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
                ffmpeg.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.RedirectStandardOutput = true;   // Redirect so we can read the standard output
                ffmpeg.StartInfo.Arguments = ffmpegArgs;
                ffmpeg.StartInfo.FileName = FFMPEG_FILENAME;

                ffmpeg.Start();
                ffmpeg.WaitForExit();
            }
            return tempfiles;
        }

        protected void runBextract(string mkcollection)
        {
            string bextractArgs = "-fe -n -ws " + WINDOW_FS + " -hp " + WINDOW_FS + " -od " + TEMP_DIRECTORY + "/" + " " + mkcollection;
            System.Diagnostics.Process bextract = new System.Diagnostics.Process();
            bextract.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            bextract.StartInfo.UseShellExecute = false;
            bextract.StartInfo.RedirectStandardOutput = false;
            bextract.StartInfo.Arguments = bextractArgs;
            bextract.StartInfo.FileName = BEXTRACT_FILENAME;

            bextract.Start();
            bextract.WaitForExit();

        }

        protected void parseArff(string arffFile)
        {
            //List of 62 value arrays.
            //One array for each song
            List<SongData> songData = new List<SongData>();

            string arff = File.ReadAllText(arffFile);
            string[] tokens = arff.Split(new[] { "% filename " }, StringSplitOptions.None);

            //Go through each of the songs (skip the first token, that has the bextract attribute comments)
            for(int i = 1; i < tokens.Length; i++)
            {
                string token = tokens[i];
                SongData sdata = new SongData();

                string[] lines = token.Split('\n');
                string filename = lines[0].Trim(new[] { '\n', '\r'});
                sdata.SetFilename(filename);
  
                //Line two is the sampling rate, which we do not use

                //Go through each of the lines of the bextract features
                for(int j = 2; j < lines.Length; j++)
                {
                    string feature_line = lines[j];
                    string[] feature_strs = feature_line.Split(',');
                    List<Double> features = new List<Double>();

                    //Convert each feature into a double, add to the double array
                    //Skip the last one, it will be 'music'
                    for(int k = 0; k < feature_strs.Length - 1; k++)
                    {
                        string feature_str = feature_strs[k];
                        Double feature = Double.Parse(feature_str);
                        features.Add(feature);
                    }

                    sdata.AddFeatureList(features);

                }
                songData.Add(sdata);
            }

        }

    }
}
