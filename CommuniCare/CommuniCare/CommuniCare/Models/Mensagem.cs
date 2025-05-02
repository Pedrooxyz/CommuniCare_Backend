/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com mensagens e chats.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Mensagem"/> representa uma mensagem dentro de um chat no sistema, incluindo o conteúdo da mensagem,
/// a data de envio e a associação ao chat ao qual pertence.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa uma mensagem dentro de um chat no sistema.
    /// </summary>
    public partial class Mensagem
    {

        #region Atributos

        /// <summary>
        /// Identificador único da mensagem.
        /// </summary>
        int mensagemId;

        /// <summary>
        /// Conteúdo da mensagem.
        /// </summary>
        string? conteudo;

        /// <summary>
        /// Data e hora de envio da mensagem.
        /// </summary>
        DateTime? dataEnvio;

        /// <summary>
        /// Identificador do chat associado à mensagem.
        /// </summary>
        int chatId;

        /// <summary>
        /// O chat ao qual a mensagem pertence.
        /// </summary>
        Chat chat = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da mensagem.
        /// </summary>
        public int MensagemId
        {
            get { return mensagemId; }
            set { mensagemId = value; }
        }

        /// <summary>
        /// Obtém ou define o conteúdo da mensagem.
        /// </summary>
        public string? Conteudo
        {
            get { return conteudo; }
            set { conteudo = value; }
        }

        /// <summary>
        /// Obtém ou define a data e hora de envio da mensagem.
        /// </summary>
        public DateTime? DataEnvio
        {
            get { return dataEnvio; }
            set { dataEnvio = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do chat ao qual a mensagem pertence.
        /// </summary>
        public int ChatId
        {
            get { return chatId; }
            set { chatId = value; }
        }

        /// <summary>
        /// Obtém ou define o chat associado à mensagem.
        /// </summary>
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