using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BankAPI.Repositories.Entities
{
    /// <summary>
    /// Transação de uma conta
    /// </summary>
    public class TransactionEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        /// <summary>
        /// Número da conta
        /// </summary>
        public int Account { get; set; }
        /// <summary>
        /// Valor da transação
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Define se é débito=true ou crédito=false o valor
        /// </summary>
        public bool IsDebit { get; set; }
        /// <summary>
        /// Data da transação
        /// </summary>
        public DateTime Date { get; set; }
    }
}
