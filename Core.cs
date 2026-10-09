using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FallenWorld
{
    public static class Core
    {
        //keys movement
        public static readonly Keys MoveLeft = Keys.A;
        public static readonly Keys MoveRight = Keys.D;
        public static readonly Keys jump = Keys.W;

        //bools for movement
        public static bool isMovingLeft = false;
        public static bool isMovingRight = false;
        public static bool isJumping = false;
        
        //keys attack
        public static readonly Keys Attack0 = Keys.D1;
        public static readonly Keys Attack1 = Keys.D2;
        public static readonly Keys Attack2 = Keys.D3;

        //bools for attack
        public static bool isAttacking0 = false;
        public static bool isAttacking1 = false;
        public static bool isAttacking2 = false;

        //keys menu
        public static readonly Keys Exit = Keys.Escape;

    }
}
