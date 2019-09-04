using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace BitBank.Core.Tests
{
    public class ContaTest
    {
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(-500.0)]
        public void Valor_deposito_deve_ser_positivo(decimal valorDeposito)
        {
            var contaDeposito = new Conta(TipoConta.ContaCorrente);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => contaDeposito.Depositar("Joao", valorDeposito)
            );
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Depositante_deve_ser_informado(string depositante)
        {
            var contaDeposito = new Conta(TipoConta.ContaCorrente);

            Assert.Throws<ArgumentNullException>(
                () => contaDeposito.Depositar(depositante, 500)
            );
        }

        [Fact]
        public void Depositar_incrementa_saldo()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 1000, "Depositado por Igor")
            };
            var contaDeposito = new Conta(transacoesBancaria,TipoConta.ContaCorrente);

            contaDeposito.Depositar("Igor", 100);
            var saldoEsperado = 1100m;
            Assert.Equal(saldoEsperado, contaDeposito.Saldo);
        }

        [Fact]
        public void Depositar_registra_transacao_bancaria()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 100, "Depositado por Igor")
            };
            var contaDeposito = new Conta(transacoesBancaria,TipoConta.ContaCorrente);

            contaDeposito.Depositar("Igor", 200);
            
            var ultimaTransacao = contaDeposito.Transacoes.Last();
            //Assert.Equal(TipoTransacao.Deposito, ultimaTransacao.Tipo);
            ultimaTransacao.Tipo.Should().Be(TipoTransacao.Deposito);
            ultimaTransacao.Valor.Should().Be(200m);
            //Assert.Equal(200m, ultimaTransacao.Valor);
        }

        [Theory]
        [InlineData(TipoConta.ContaCorrente, -400)]
        [InlineData(TipoConta.ContaPoupanca, -100)]
        public void Saldo_conta_corrente_ou_poupanca_nao_deve_exceder_cheque_especial(TipoConta tipo, decimal valor)
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, valor, "Depositado por Igor")
            };
            
            Assert.Throws<Exception>(
                () => new Conta(transacoesBancaria, tipo)
            );
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(-500.0)]
        public void Valor_saque_deve_ser_positivo(decimal valorSaque)
        {
            var contaSaque = new Conta(TipoConta.ContaCorrente);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => contaSaque.Saque(valorSaque)
            );
        }
    }
}