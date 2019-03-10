﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class PasswordReset
    {
        public PasswordReset()
        {
            Id = Guid.NewGuid();
            ExpirationTime = DateTime.Now.AddMinutes(5);
            ResetCount = 0;
            Disabled = false;
        }

        [Required, Key]
        public Guid Id { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required, ForeignKey("User")]
        public Guid UserID { get; set; }
        public User User { get; set; }

        [Required, Column(TypeName = "datetime2"), DataType(DataType.DateTime)]
        public DateTime ExpirationTime { get; set; }

        [Required]
        //Variable to keep track of how many attempts were made with this resetToken
        public int ResetCount { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [Required]
        public bool AllowPasswordReset { get; set; }
    }
}
