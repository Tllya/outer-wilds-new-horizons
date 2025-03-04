﻿using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
namespace NewHorizons.Utility
{
    [JsonObject]
    public class MColor
    {
        public MColor(int r, int g, int b, int a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        /// The red component of this colour
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0f, int.MaxValue)] 
        public int r;

        /// <summary>
        /// The green component of this colour
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0f, int.MaxValue)] 
        public int g;
        
        /// <summary>
        /// The blue component of this colour
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0f, int.MaxValue)] 
        public int b;
        
        /// <summary>
        /// The alpha (opacity) component of this colour
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0f, 255f)] 
        [DefaultValue(255f)]
        public int a;

        public Color ToColor() => new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
