﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Permission { get; set; }

        public User(string email, string password,
            bool permission)
        {
            Email = email;
            Password = password;
            Permission = permission;
        }
    }
}