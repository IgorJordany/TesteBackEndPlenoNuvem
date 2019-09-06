using System;

namespace BitBank.Core
{
    public enum TipoTransacao
    {
        Deposito,
        Saque,
        Tarifa
    }
    
    public class TransacaoBancaria
    {
        public TransacaoBancaria(DateTime data, TipoTransacao tipo, decimal valor, string descricao)
        {
            Data = data;
            Tipo = tipo;
            Valor = valor;
            Descricao = descricao;
        }
        
        public DateTime Data { get; }
        public TipoTransacao Tipo { get; }
        public decimal Valor { get; }
        public string Descricao { get; }
    }
}