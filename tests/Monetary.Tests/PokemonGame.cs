using AutoFixture;
using FluentAssertions;
using Monetary.Tests.Features.Units;
using System.Collections;

namespace Monetary.Tests;

public class PokemonGame
{
    public PokemonGame()
    {
        Player = new Player();
        CPU = new Player();
    }

    public Player Winner { get; private set; }
    public Player Player { get; private set; }
    public Player CPU { get; private set; }

    public bool CanStart
    {
        get
        {
            return
                Player.Character is not null && CPU.Character is not null;
        }
    }

    internal void Start()
    {
        do
        {
            Turno();
        } while (Player.HP > 0 && CPU.HP > 0);
        DefinirVencedor();
    }

    private void DefinirVencedor()
    {
        Winner = CPU.HP <= 0 ? Player : CPU;
    }

    private void Turno()
    {
        //CPU Ataca
        CPU.CausarDano(Player);
        //Player Ataca
        Player.CausarDano(CPU);
    }
}

public class Player
{
    public int HP { get; set; } = 100;
    public Character Character { get; set; }

    internal void CausarDano(Player player)
    {
        player.HP -= Character.Attack - (int)Math.Round(this.Character.Defense);
    }

    internal void SelectCharacter(Character character)
    {
        Character = character;
    }

    internal void SelectCharacter(string name, decimal defense, int attack)
    {
        Character = new Character(name, defense, attack);
    }
}

public class Character
{
    public Character(string name, decimal defense, int attack)
    {
        Name = name;
        Defense = defense;
        Attack = attack;
    }

    public string Name { get; private set; }
    public decimal Defense { get; private set; }
    public int Attack { get; private set; }
}

public static class CharacterBuilder
{
    public static Character Pikachu()
    {
        return new Character("Pikachu", 0M, 20);
    }

    public static Character Psyduck()
    {
        return new Character("Psyduck", 0M, 10);
    }
}

public class PokemonGameUnitTests : UnitTests
{
    private readonly PokemonGame _sut;

    public PokemonGameUnitTests()
    {
        _sut = _fixture.Create<PokemonGame>();
    }

    [Fact]
    public async Task ValidarHPInicial()
    {
        // Assert
        _sut.Player.HP.Should().Be(100);
        _sut.CPU.HP.Should().Be(100);
        _sut.CanStart.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmarPokemonInicial()
    {
        // Act
        _sut.Player.SelectCharacter(CharacterBuilder.Pikachu());
        _sut.CPU.SelectCharacter(CharacterBuilder.Psyduck());
        // Assert
        _sut.Player.Character.As<Character>().Name.Should().Be("Pikachu");
        _sut.CPU.Character.As<Character>().Name.Should().Be("Psyduck");
    }

    [Fact]
    public async Task VerificarSeLutaPodeSerIniciada()
    {
        // Act
        _sut.Player.SelectCharacter(CharacterBuilder.Pikachu());
        // Assert
        _sut.CanStart.Should().BeFalse();
        // Act
        _sut.CPU.SelectCharacter(CharacterBuilder.Psyduck());
        // Assert
        _sut.CanStart.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(BattleBuilder))]
    public async Task RealizarLuta(Character player, Character cpu, string winner)
    {
        // Act
        _sut.Player.SelectCharacter(player);
        _sut.CPU.SelectCharacter(cpu);
        _sut.Start();
        // Assert
        _sut.Winner.Character.Name.Should().Be(winner);
    }
}

public class BattleBuilder : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            CharacterBuilder.Pikachu(),
            CharacterBuilder.Psyduck(),
            CharacterBuilder.Pikachu().Name
        };
        yield return new object[]
        {
            new Character("Ash", 0, 50),
            new Character("Boss", 0.1M, 15),
            "Ash"
        };
        yield return new object[]
        {
            new Character("Charmander", 0.05M, 5),
            new Character("Blastoise", 0.95M, 45),
            "Blastoise"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}