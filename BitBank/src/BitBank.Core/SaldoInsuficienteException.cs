using System;

namespace BitBank.Core
{
    public class SaldoInsuficienteException : Exception
    {
        public decimal Saldo { get;}
        public decimal Valor { get; }
        public decimal LimiteChequeEspecial { get; }
        public SaldoInsuficienteException(decimal saldo, decimal valor, decimal limiteChequeEspecial) : base("Saldo insuficiente")
        {
            Saldo = saldo;
            Valor = valor;
            LimiteChequeEspecial = limiteChequeEspecial;
        }
    }
}