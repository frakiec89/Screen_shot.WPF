using System;

namespace Screen_shot.WPF.Model
{
    /// <summary>
    /// стуктура отправки  получения запроса  для  апи
    /// </summary>
    public class ImagePost
    {
        public byte[] bytes { get; set; }
        public string key { get; set; }
        public DateTime timeShet { get; set; }
    }

}
