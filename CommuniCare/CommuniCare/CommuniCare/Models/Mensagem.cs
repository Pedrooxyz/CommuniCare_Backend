using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Mensagem
    {

        #region Atributos

        int mensagemId;

        string? conteudo;

        DateTime? dataEnvio;

        int chatId;

        Chat chat = null!;

        #endregion

        #region Propriedades

        public int MensagemId
        {
            get { return mensagemId; }
            set { mensagemId = value; }
        }

        public string? Conteudo
        {
            get { return conteudo; }
            set { conteudo = value; }
        }

        public DateTime? DataEnvio
        {
            get { return dataEnvio; }
            set { dataEnvio = value; }
        }

        public int ChatId
        {
            get { return chatId; }
            set { chatId = value; }
        }

        public virtual Chat Chat
        {
            get { return chat; }
            set { chat = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}