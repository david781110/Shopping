using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shopping.Models
{
   
    public class TestCart
    {
      [System.ComponentModel.DataAnnotations.Key]
       public int TestCartID { get; set; }

        public string? PID { get; set; }

        public int QTY { get; set; }

        public string? CID { get; set;}
    }
}
