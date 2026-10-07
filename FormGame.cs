using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FallenWorld
{
    public partial class FormGame : Form
    {
        public FormGame(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
        }

        #region Global Variables
        private readonly Personagem _personagem;
        #endregion

        private void FormGame_Load(object sender, EventArgs e)
        {
            switch (_personagem)
            {
                case Personagem.Mago: panel1.BackgroundImage = Properties.Resources.duckMage_Avatar_normal; break;
                case Personagem.Cavaleiro: panel1.BackgroundImage = Properties.Resources.knight_Avatar_normal; break;
                case Personagem.Princesa: panel1.BackgroundImage = Properties.Resources.Princess_Avatar_normal; break;
            }

        }


        #region Controles

        #endregion

        #region Perda de vida e morte do personagem
        #endregion
    }
}
