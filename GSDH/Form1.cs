using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GSD_HASH;

namespace GSDH
{
    public partial class Form1 : Form
    {

        private string connectionString;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = "Data Source=10.125.50.191;Initial Catalog=Logins;User ID=aluno;Password=aluno;";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Abrindo a conexão
                    connection.Open();
                    MessageBox.Show("Conexão com o banco de dados bem-sucedida!");


                }
                catch (Exception ex)
                {
                    // Se ocorrer algum erro, mostramos a mensagem de erro
                    MessageBox.Show("Erro ao conectar ao banco de dados: " + ex.Message);
                }
                finally
                {
                    // A conexão será fechada automaticamente pelo 'using'
                }
            }
        }



        private void btnRegistro_Click(object sender, EventArgs e)
        {
            AdicionarUsuario(txtUsuarioR.Text, txtSenhaR.Text, txtConfirmar.Text, txtEmail.Text);

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuarioLogin.Text.Trim();
            string senha = Crypto.sha256encrypt(txtUsuarioSenha.Text.Trim());
            string connectionString = "Data Source=10.125.50.191;Initial Catalog=Logins;User ID=aluno;Password=aluno;";
            string query = "SELECT COUNT(*) FROM LoginsJV WHERE usuario = @usuario AND senha = @senha";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@senha", senha);

                conn.Open();

                int resultado = (int)cmd.ExecuteScalar();

                if (resultado > 0)
                {
                    txtUsuarioLogin.Text = String.Empty;
                    txtUsuarioSenha.Text = String.Empty;
                    MessageBox.Show("Login realizado com sucesso!");
                }
                else
                {
                    MessageBox.Show("Usuário/Senha incorretos");
                }
            }
        }

        private void AdicionarUsuario(string _nomeUsuario, string _senha, string _confirmaSenha, string _email)
        {

            // Regex para validar o email
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(_email);

            //// Verifica se o nome de usuário já existe no banco de dados
            //if (usu(_nomeUsuario))
            //{
            //    MessageBox.Show("O nome do usuário já existe, tente informar outro nome.");
            //    return;
            //}

            // Confirma a senha
            if (_senha != _confirmaSenha)
            {
                MessageBox.Show("A senha não confere.");
            }
            // A senha tem que ter no mínimo 8 caracteres
            else if (_senha.Length < 8)
            {
                MessageBox.Show("A senha deve conter no mínimo 8 caracteres");
            }
            // Se o email não for válido
            else if (!match.Success)
            {
                MessageBox.Show("Email inválido");
            }
            // Se não informou o usuário
            else if (_nomeUsuario == null)
            {
                MessageBox.Show("Você deve informar um usuário");
            }
            // Se estiver tudo certo, então cria o usuário
            else
            {
                // Criptografa a senha
                string _hashSenha = Crypto.sha256encrypt(_senha);

                AdicionaUsuarioNoBD(_nomeUsuario, _hashSenha, _email);

                // Limpa os campos do formulário
                txtUsuarioR.Text = String.Empty;
                txtSenhaR.Text = String.Empty;
                txtConfirmar.Text = String.Empty;
                txtEmail.Text = String.Empty;

                MessageBox.Show("Obrigado por seu registro!");
            }
        }

        private void AdicionaUsuarioNoBD(string _nomeUsuario, string _senha, string _email)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))  // AQUI você passa a connectionString
            {
                try
                {
                    // Abrindo a conexão
                    cn.Open();

                    // Definindo o comando SQL para inserir o usuário
                    string sql = "INSERT INTO LoginsJV (usuario, senha, email) VALUES (@usuario, @senha, @email)";

                    // Criando o comando SQL com parâmetros
                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        // Adicionando os parâmetros
                        cmd.Parameters.Add(new SqlParameter("@usuario", SqlDbType.NChar)).Value = _nomeUsuario;
                        cmd.Parameters.Add(new SqlParameter("@senha", SqlDbType.NChar)).Value = _senha;
                        cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NChar)).Value = _email;

                        // Executando o comando no banco de dados
                        cmd.ExecuteNonQuery();
                    }

                    // Mensagem de sucesso
                    MessageBox.Show("Usuário adicionado com sucesso.");
                }
                catch (SqlException sqlexception)
                {
                    // Tratamento de erro
                    MessageBox.Show(sqlexception.Message, "Erro ao adicionar usuário", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // Tratamento de erro genérico
                    MessageBox.Show(ex.Message, "Erro desconhecido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // A conexão será fechada automaticamente pelo 'using'
                }

            }
        }
        }

    }
