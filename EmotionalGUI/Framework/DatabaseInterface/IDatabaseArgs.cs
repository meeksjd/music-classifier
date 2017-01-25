﻿
namespace Framework
{
    interface IDatabaseArgs
    {
        /// <summary>
        /// Get a list of X of the closest matching songs, DTO is a coordinate in emotionspace
        /// </summary>
        /// <param name="numberOfSongs">The number of songs to make a playlist out of</param>
        /// <param name="emotionSpaceDTO"></param>
        /// <returns></returns>
        string[] getSongs(int numberOfSongs, EmotionSpaceDTO emotionSpaceDTO);

        /// <summary>
        /// Gets a list of X songs that fall in between the min and max values
        /// </summary>
        /// <param name="numberOfSongs"></param>
        /// <param name="minValueEmotionSpaceDTO"></param>
        /// <param name="maxValueEmotionSpaceDTO"></param>
        /// <returns></returns>
        string[] getSongWithConstraints(int numberOfSongs, EmotionSpaceDTO minValueEmotionSpaceDTO, EmotionSpaceDTO maxValueEmotionSpaceDTO);
    }
}