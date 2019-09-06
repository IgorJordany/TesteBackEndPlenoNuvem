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
        const decimal ValorTarifaSaque = -4.77m;
        private readonly List<TransacaoBancaria> _transacoes;
        public decimal LimiteChequeEspecial { get; }

        public Conta(IEnumerable<TransacaoBancaria> transacoes, TipoConta tipo, decimal limiteChequeEspecial = 0)
        {
            if (tipo == TipoConta.ContaPoupanca && limiteChequeEspecial != 0)
            {
                throw new ArgumentException(nameof(limiteChequeEspecial), "limiteChequeEspecial");
            }

            _transacoes = transacoes?.ToList() ?? new List<TransacaoBancaria>();
            Tipo = tipo;
            LimiteChequeEspecial = - limiteChequeEspecial;
        }

        public Conta(TipoConta tipo, decimal limiteChequeEspecial = 0)
        {
            if (tipo == TipoConta.ContaPoupanca && limiteChequeEspecial != 0)
            {
                throw new ArgumentException(nameof(limiteChequeEspecial), "limiteChequeEspecial");
            }

            _transacoes =  new List<TransacaoBancaria>();
            Tipo = tipo;
            LimiteChequeEspecial = - limiteChequeEspecial;
        }

        public ReadOnlyCollection<TransacaoBancaria> Transacoes => _transacoes.AsReadOnly();

        public TipoConta Tipo { get; }
        public decimal Saldo => _transacoes.Sum(t => t.Valor);

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
        public void Sacar(decimal valor)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), valor, "O valor do saque deve ser positivo");
            }

            if (Saldo - valor < LimiteChequeEspecial)
            {
                throw new SaldoInsuficienteException(Saldo, valor, LimiteChequeEspecial);
            }

            var possuiSaqueDentroDoMes = _transacoes.Any(t => t.Tipo == TipoTransacao.Saque && t.Data.Month == DateTime.Now.Month && t.Data.Year == DateTime.Now.Year);
            
            _transacoes.Add(new TransacaoBancaria(DateTime.Now, TipoTransacao.Saque, - valor, $"Saque em terminal"));

            if (Tipo != TipoConta.ContaPoupanca && possuiSaqueDentroDoMes)
            {
                _transacoes.Add(new TransacaoBancaria(DateTime.Now, TipoTransacao.Tarifa, ValorTarifaSaque, $"Tarifa Saque"));
            }
        }
    }
}
