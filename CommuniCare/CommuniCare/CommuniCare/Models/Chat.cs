using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Chat
    {
        #region Atributos

        int chatId;

        int utilizadorId;

        ICollection<Mensagem> mensagems = new List<Mensagem>();
        
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        public int ChatId
        {
            get { return chatId; }
            set { chatId = value; }
        }

        public int UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public virtual ICollection<Mensagem> Mensagems
        {
            get { return mensagems; }
            set { mensagems = value; }
        }

        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        #endregion

        #region Construtores

        #endregion
    }
}