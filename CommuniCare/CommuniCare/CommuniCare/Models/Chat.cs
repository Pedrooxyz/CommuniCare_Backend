/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades que representam
/// os chats, mensagens e utilizadores envolvidos nas interações de comunicação no sistema.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Chat"/> representa um chat entre um utilizador e o sistema, contendo uma lista
/// de mensagens trocadas. Cada chat está associado a um utilizador através do <see cref="Utilizador"/> 
/// e possui um identificador único.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa um chat entre um utilizador e o sistema.
    /// </summary>
    public partial class Chat
    {

        #region Atributos

        /// <summary>
        /// Identificador único do chat.
        /// </summary>
        int chatId;

        /// <summary>
        /// Identificador do utilizador associado ao chat.
        /// </summary>
        int utilizadorId;

        /// <summary>
        /// Lista de mensagens trocadas no chat.
        /// </summary>
        ICollection<Mensagem> mensagems = new List<Mensagem>();
        
        /// <summary>
        /// Utilizador associado ao chat.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do chat.
        /// </summary>
        public int ChatId
        {
            get { return chatId; }
            set { chatId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do utilizador associado ao chat.
        /// </summary>
        public int UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de mensagens trocadas no chat.
        /// </summary>
        public virtual ICollection<Mensagem> Mensagems
        {
            get { return mensagems; }
            set { mensagems = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador associado ao chat.
        /// </summary>
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