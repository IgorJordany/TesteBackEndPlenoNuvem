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

        [Fact]
        public void Saldo_deve_ser_somatorio_das_transacoes()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 1000, "Saldo inicial"),
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Saque, -500, "Saque terminal"),
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 800, "Deposito de Joao")
            };
            var conta = new Conta(transacoesBancaria,TipoConta.ContaCorrente);

            var saldoEsperado = 1300m;
            conta.Saldo.Should().Be(saldoEsperado);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(-500.0)]
        public void Valor_saque_deve_ser_positivo(decimal valorSaque)
        {
            var contaSaque = new Conta(TipoConta.ContaCorrente);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => contaSaque.Sacar(valorSaque)
            );
        }

        [Fact]
        public void Sacar_registra_transacao_bancaria_de_saque()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 500, "Saldo inicial")
            };
            var contaSaque = new Conta(transacoesBancaria,TipoConta.ContaCorrente);
            var valorSaque = 200;

            contaSaque.Sacar(valorSaque);
            var ultimaTransacao = contaSaque.Transacoes.Last();

            if (ultimaTransacao.Tipo == TipoTransacao.Tarifa)
            {
                var antepenultimaTransacao = contaSaque.Transacoes.ElementAt(contaSaque.Transacoes.Count - 2);
                antepenultimaTransacao.Tipo.Should().Be(TipoTransacao.Saque);
                antepenultimaTransacao.Valor.Should().Be(-valorSaque);
            }
            else
            {
                ultimaTransacao.Tipo.Should().Be(TipoTransacao.Saque);
                ultimaTransacao.Valor.Should().Be(-valorSaque);
            }
        }

        [Fact]
        public void Sacar_deve_aplicar_de_tarifa_saque_se_nao_for_primeira_operacao_de_saque_do_mes()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Deposito, 1000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Saque, -100, "Saque terminal")
            };

            var contaSaque = new Conta(transacoesBancaria, TipoConta.ContaCorrente);
            var valorTarifaSaque = -4.77m;
            var valorSaque = 123m;

            contaSaque.Sacar(valorSaque);

            var antepenultimaTransacao = contaSaque.Transacoes.ElementAt(contaSaque.Transacoes.Count - 2);
            var ultimaTransacao = contaSaque.Transacoes.Last();

            antepenultimaTransacao.Tipo.Should().Be(TipoTransacao.Saque);
            antepenultimaTransacao.Valor.Should().Be(-valorSaque);

            ultimaTransacao.Tipo.Should().Be(TipoTransacao.Tarifa);
            ultimaTransacao.Valor.Should().Be(valorTarifaSaque);
        }

        [Fact]
        public void Sacar_nao_deve_aplicar_tarifa_se_for_primeira_operacao_de_saque_do_mes()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Deposito, 1000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -500, "primeiro Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -300, "Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Tarifa, -4.77m, "tarifa")
            };

            var contaSaque = new Conta(transacoesBancaria, TipoConta.ContaCorrente);
            var valorSaque = 123m;

            contaSaque.Sacar(valorSaque);

            var ultimaTransacao = contaSaque.Transacoes.Last();
            ultimaTransacao.Tipo.Should().Be(TipoTransacao.Saque);
            ultimaTransacao.Valor.Should().Be(-valorSaque);
        }

        [Fact]
        public void Sacar_nao_debita_tarifa_em_conta_poupanca()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Deposito, 10000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -500, "primeiro Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -300, "Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Tarifa, -4.77m, "tarifa"),
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Saque, -300, "Saque"),
                new TransacaoBancaria(DateTime.Now, TipoTransacao.Tarifa, -4.77m, "tarifa")
            };

            var contaSaque = new Conta(transacoesBancaria, TipoConta.ContaPoupanca);
            var valorSaque = 123m;

            contaSaque.Sacar(valorSaque);

            var ultimaTransacao = contaSaque.Transacoes.Last();
            ultimaTransacao.Tipo.Should().Be(TipoTransacao.Saque);
            ultimaTransacao.Valor.Should().Be(-valorSaque);
        }

        [Fact]
        public void Conta_corrente_deve_sacar_do_cheque_especial()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Deposito, 1000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -800, "primeiro Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -200, "Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Tarifa, -4.77m, "tarifa")
            };

            var limiteChequeEspecial = 300m;
            var contaSaque = new Conta(transacoesBancaria, TipoConta.ContaCorrente, limiteChequeEspecial);
            var valorSaque = 200m;

            contaSaque.Sacar(valorSaque);

            contaSaque.Saldo.Should().Be(-204.77m);
        }

        [Fact]
        public void Conta_corrente_nao_deve_sacar_mais_do_que_limite_cheque_especial()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Deposito, 1000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -800, "primeiro Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -200, "Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Tarifa, -4.77m, "tarifa")
            };

            var limiteChequeEspecial = 300m;
            var contaSaque = new Conta(transacoesBancaria, TipoConta.ContaCorrente, limiteChequeEspecial);
            var valorSaque = 295.24m;

            Action action = () => contaSaque.Sacar(valorSaque);

            action.Should().ThrowExactly<SaldoInsuficienteException>();
        }

        [Fact]
        public void Conta_poupanca_nao_deve_possui_cheque_especial()
        {
            var transacoesBancaria = new[] { 
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Deposito, 1000, "Deposito Joao"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -800, "primeiro Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Saque, -200, "Saque"),
                new TransacaoBancaria(DateTime.Now.AddMonths(-1), TipoTransacao.Tarifa, -4.77m, "tarifa")
            };

            var limiteChequeEspecial = 300m;

            Action action = () => new Conta(transacoesBancaria, TipoConta.ContaPoupanca, limiteChequeEspecial);

            action.Should().ThrowExactly<ArgumentException>()   
                .And.ParamName.Should().Be("limiteChequeEspecial");
        }
    }
}