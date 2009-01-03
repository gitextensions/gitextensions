using System;
using System.Collections.Generic;

using System.Text;

namespace GitCommands
{
    public class CheckoutDto
    {
        public string Name { get; set; }
        public string Result { get; set; }

        public CheckoutDto(string name)
        {
            this.Name = name;
        }
    }

    public class Checkout
    {
        public CheckoutDto Dto { get; set; }
        public Checkout(CheckoutDto dto)
        {
            this.Dto = dto;
        }

        public void Execute()
        {
            Dto.Result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "checkout " + Dto.Name);
        }
    }
}
