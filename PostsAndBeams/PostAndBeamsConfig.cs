using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostsAndBeams
{
    public class PostsAndBeamsConfig
    {
        public static PostsAndBeamsConfig Loaded { get; set; } = new PostsAndBeamsConfig();

        public int MaxDistanceBeamFromPostBlocks { get; set; } = 3;
    }
}