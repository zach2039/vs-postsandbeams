using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace postsandbeams
{
    public class PostsAndBeamsConfig
    {
        public static PostsAndBeamsConfig Loaded { get; set; } = new PostsAndBeamsConfig();

        public int MaxDistanceBeamFromPostBlocks { get; set; } = 3;
    }
}