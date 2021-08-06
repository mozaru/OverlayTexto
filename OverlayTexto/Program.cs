using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace OverlayTexto
{
    class Program
    {
        private string texto;
        private Point coordenada;
        private string origem;
        private string destino;
        private Font fonte;
        private Brush cor;

        private Dictionary<string,string> ParseArgs(string []args, params string []keys)
        {
            string key=null;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach(string str in args)
            {
                if (key == null)
                    if (Array.Exists(keys, (x => x.Equals(str, StringComparison.OrdinalIgnoreCase))))
                        key = Array.Find(keys, (x => x.Equals(str, StringComparison.OrdinalIgnoreCase)));
                    else
                        throw new Exception($"chave {str} nao permitida!");
                else if (dic.ContainsKey(key))
                    throw new Exception($"chave {key} encontrada mais de uma vez!");
                else if (Array.Exists(keys, (x => x.Equals(str, StringComparison.OrdinalIgnoreCase))))
                    throw new Exception($"chave {key} encontrada como valor de outra chave!");
                else
                {
                    dic.Add(key, str);
                    key = null;
                }
            }
            return dic;
        }

        private static string ObterValor(Dictionary<string, string> dic, string key, string valor=null)
        {
            if (valor == null && !dic.ContainsKey(key))
                throw new Exception($"A chave {key} é obrigatoria!");
            else if (dic.ContainsKey(key))
                return dic[key];
            else
                return valor;
        }

        private void Inicializar(string []args)
        {
            FontStyle estilo;
            int tamanho;
            string nomeFonte;
            estilo = FontStyle.Bold;
            Dictionary<string, string> dic = ParseArgs(args, "-f", "-t", "-x", "-y", "-s", "-i", "-o", "-c");
            nomeFonte = ObterValor(dic, "-f", "Arial");
            texto = ObterValor(dic,"-t");
            coordenada = new Point(int.Parse(ObterValor(dic,"-x","0")), int.Parse(ObterValor(dic, "-y", "0")));
            tamanho = int.Parse(ObterValor(dic, "-s", "12"));
            origem = ObterValor(dic, "-i");
            destino = ObterValor(dic, "-o");
            cor = new SolidBrush(Color.FromArgb(int.Parse(ObterValor(dic, "-c", "0").Replace("#",""), System.Globalization.NumberStyles.HexNumber)));
            fonte = new Font(nomeFonte, tamanho, estilo);
        }

        private void AplicarOverlay()
        {
            if (!File.Exists(origem))
                throw new Exception($"Arquivo de entrada {origem} nao encontrado!");

            if (!Directory.Exists(Path.GetDirectoryName(destino)))
                throw new Exception($"Diretorio de destino {Path.GetDirectoryName(destino)} nao encontrado!");

            using (Bitmap bmp = new Bitmap(origem))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.TextContrast = 10;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.DrawString(texto, fonte, cor, coordenada);
                }
                bmp.Save(destino);
            }
        }
        static void Main(string[] args)
        {
            Program p = new Program();
            try
            {
                p.Inicializar(args);
                p.AplicarOverlay();
                Console.WriteLine("Arquivo {0} Gerado com sucesso!", p.destino);
            }
            catch (Exception e)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                string exeName = Path.GetFileName(codeBase);
                Console.WriteLine("Erro encontrado!");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Sintaxe de utilizacao:");
                Console.WriteLine($"{exeName} [-f|-t|-x|-y|-s|-i|-o|-c]");
                Console.WriteLine("onde:\n-f: nome da fonte\n-t: texto\n-x: posicao x\n-y:posicao y\n-s: tamanho do texto\n-c: cor da fonte\n-i: caminho da imagem de entrada\n-o: camanho da imagem a ser gerada\n\n");
                Console.WriteLine($"ex:{exeName} -f arial -t \"hello word\" -x 10 -y 10 -s 16 -c #000000 -i \"C:\\imagens\\teste.png\" -o C:\\imagens\\teste_output.png\n\n");
            }
        }
    }
}
