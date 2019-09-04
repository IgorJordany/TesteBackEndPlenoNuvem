using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace BitBank.Core
{
    public enum TipoConta
    {
        ContaCorrente,
        ContaPoupanca
    }

    public class Conta
    {
        private readonly List<TransacaoBancaria> _transacoes = new List<TransacaoBancaria>();

        public Conta(IEnumerable<TransacaoBancaria> transacoes, TipoConta tipo)
        {
            _transacoes = transacoes?.ToList();
            Tipo = tipo;
            var saldoInicial = _transacoes.Sum(t => t.Valor);
            LimiteSaldoExcedido(saldoInicial,tipo);
        }

        public Conta(TipoConta tipo)
        {
            _transacoes =  new List<TransacaoBancaria>();
            Tipo = tipo;
        }

        public ReadOnlyCollection<TransacaoBancaria> Transacoes => _transacoes.AsReadOnly();

        public TipoConta Tipo { get; }
        public decimal Saldo => _transacoes.Sum(t => t.Valor);
        private void LimiteSaldoExcedido(decimal saldoInicial, TipoConta tipo)
        {
            if (tipo == TipoConta.ContaCorrente)
                if (saldoInicial < -300)
                    throw new Exception("Saldo conta corrente excedeu o limite");

            if (tipo == TipoConta.ContaPoupanca)
                if (saldoInicial < 0)
                    throw new Exception("Saldo conta poupança excedeu o limite");
        }
        public void Depositar(string depositante, decimal valor)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), valor, "O valor do deposito deve ser positivo");
            }
            if (string.IsNullOrWhiteSpace(depositante))
            {
                throw new ArgumentNullException(nameof(depositante), "Depositante deve ser informado");
            }

            _transacoes.Add(new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, valor, $"Depositante {depositante}"));
        }
        public void Saque(decimal valor)
        {

        }
    }
}
