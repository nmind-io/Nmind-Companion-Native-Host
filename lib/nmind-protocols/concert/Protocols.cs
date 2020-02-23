//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using Nmind.IO;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 
/// </summary>
namespace Nmind.Protocols.Concert {

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ProtocolException : Exception {

        public ProtocolException() {
        }

        public ProtocolException(string message) : base(message) {
        }

        public ProtocolException(string message, Exception inner) : base(message, inner) {
        }

        protected ProtocolException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ConcertProtocol {

        /// <summary>
        /// 
        /// </summary>
        public static readonly string PROTOCOL_V2_NAME = "Concert V2";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string PROTOCOL_V2_ID = "Concert-V2";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string PROTOCOL_V3_NAME = "Concert V3";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string PROTOCOL_V3_ID = "Concert-V3";

        /// <summary>
        /// Message start
        /// </summary>
        public static readonly byte STX = 0x02;

        /// <summary>
        /// Message end
        /// </summary>
        public static readonly byte ETX = 0x03;

        /// <summary>
        /// Session start
        /// </summary>
        public static readonly byte EOT = 0x04;

        /// <summary>
        /// Ask for open session
        /// </summary>
        public static readonly byte ENQ = 0x05;

        /// <summary>
        /// Positive aknowledgement
        /// </summary>
        public static readonly byte ACK = 0x06;

        /// <summary>
        /// Negative aknowledgement
        /// </summary>
        public static readonly byte NAK = 0x0D;

        /// <summary>
        /// Euro
        /// </summary>
        public static readonly int CURRENCY_EURO = 978;

        /// <summary>
        /// 
        /// </summary>
        public static readonly byte TRANSACTION_TYPE_PURCHASE = 0;

        /// <summary>
        /// 
        /// </summary>
        public static readonly byte TRANSACTION_TYPE_REFUND = 0x01;

        /// <summary>
        /// 
        /// </summary>
        public static readonly byte TRANSACTION_TYPE_CANCEL = 0x02;

        /// <summary>
        /// 
        /// </summary>
        public static readonly char STATUS_SUCCESS = (char)'0';

        /// <summary>
        /// 
        /// </summary>
        public static readonly char STATUS_FAILED = (char)'7';

        /// <summary>
        /// 
        /// </summary>
        public static readonly char MODE_CARD = (char)'1';

        /// <summary>
        /// 
        /// </summary>
        public static readonly char IND_RESPONSE = (char)'1';

        /// <summary>
        /// 
        /// </summary>
        public static int WAITING_TIME = 60 * 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, string> SupportedProtocols() {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add(PROTOCOL_V2_ID, PROTOCOL_V2_NAME);
            map.Add(PROTOCOL_V3_ID, PROTOCOL_V3_NAME);
            return map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FromCharCode(int code) {
            char[] chars = Encoding.ASCII.GetChars(new byte[1] { (byte)code });
            return chars[0].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBytes(string data) {
            return Encoding.ASCII.GetBytes(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte LRC(string message) {
            return LRC(ToBytes(message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte LRC(byte[] bytes) {
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++) {
                LRC ^= bytes[i];
            }
            return LRC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ConcertProtocol find(string name) {

            if (String.Equals(name, PROTOCOL_V2_ID)){
                return new Concert2Protocol();
            } else if(String.Equals(name, PROTOCOL_V3_ID)){
                 throw new ProtocolException("Unimplemented protocol " + name);
            } else {
                throw new ProtocolException("Unimplemented protocol " + name);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public Terminal Terminal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ConcertProtocol() {

        }

        /// <summary>
        /// 
        /// </summary>
        public ConcertProtocol(Terminal terminal) {
            this.Terminal = terminal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract String ComposeMessage(PaymentRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Issue SendTest();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Issue SendPaymentRequest(PaymentRequest request);

    }

    /// <summary>
    /// 
    /// </summary>
    public class Concert2Protocol : ConcertProtocol {

        /// <summary>
        /// 
        /// </summary>
        public Concert2Protocol() {

        }

        /// <summary>
        /// 
        /// </summary>
        public Concert2Protocol(Terminal terminal) : base(terminal) {
  
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Issue SendTest() {
            Issue issue = null;

            Terminal.Connect();
            Terminal.Write(ENQ);

            if (Terminal.ReadOne() != ACK) {
                issue = Issue.Failure("Terminal did not respond ACK after login");
            } else {
                Terminal.Write(EOT);
                issue = Issue.Success("");
            }

            Terminal.Disconnect();
            return issue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// Structure du message E
        ///
        /// xx 			: N° Caisse : Identifiant de la caisse
        /// xxxxxxxx 	: MONTANT	: Montant exprimé dans la plus petite unité de fractionnement 
        /// 									complété à gauche par des zéros. Indiquer un montant positif.
        /// x			: IND		: Présence du champ REP de la réponse 
        /// 									= 1 → champ REP envoyé dans la réponse 
        /// 									= autre → champ REP non envoyé
        /// x			: MODE		: Mode de règlement, 
        /// 									= 1 → traitement carte uniquement, 
        /// 									= C → traitement chèque (si pas d'appli chèque, message "FONCTIONNON IMPLEMENTEE ") 
        /// 									= autre → choix "CARTE CHEQUE"
        /// x			: TYPE		: Type de transaction
        ///									= 0 → (achat) traitement du débit
        ///									= 1 → (remboursement) traitement du crédit (TM7783400)
        ///									= 2 → annulation (disponible à partir de TM7783400)
        ///									= 4 → pré-autorisation (disponible à partir de TM7784200)
        ///									= autre → message "FONCTION NON IMPLEMENTEE ")
        /// 									= autre → choix "CARTE CHEQUE"
        /// xxx			: DEVISE	: Code numérique de la devise
        ///									(pour info: 250 pour FRF, 978 pour EUR)
        ///									= code numérique connu → traitement du débit dans la devise spécifiée
        ///									= code numérique inconnu → affichage "FONCTION IMPOSSIBLE"
        /// 									= autre → choix "CARTE CHEQUE"
        /// xxxxxxxxxx	: PRIVE		: Données privées remontées par l’application.
        ///
        /// <param name="request"></param>
        /// <returns></returns>
        public override String ComposeMessage(PaymentRequest request) {

            string prive = request.Data;
            if (prive.Length > 10) {
                prive = prive.Substring(0, 10);
            }
            prive = prive.PadRight(10, ' ');

            String message = String.Format(
                "{0:d2}{1:d8}{2:d1}{3:d1}{4:d1}{5:d3}{6}",
                request.Pos,
                (int)(request.Amount * 100),
                request.Ind,
                request.Mode,
                request.Type,
                request.Currency,
                prive
            );

            return message;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected byte[] PrepareMessage(PaymentRequest request) {
            string payload = ComposeMessage(request) + FromCharCode(ETX).ToString();

            string message = FromCharCode(STX);
            message += payload;
            message += FromCharCode(LRC(payload));

            return ToBytes(message);
        }

        /// <summary>
        /// 
        /// </summary>
        ///
        /// Structure d'une réponse E
        ///
        /// xx 			: N° Caisse : Identifiant de la caisse
        /// x			: STATUT	: Etat de la transaction
        /// 									= 0 → si transaction acceptée
        /// 									= 7 → si transaction non aboutie
        /// xxxxxxxx 	: MONTANT	: Montant exprimé dans la plus petite unité de fractionnement 
        /// 									complété à gauche par des zéros. Indiquer un montant positif.
        /// x			: MODE		: Mode de règlement, 
        /// 									= 1 → traitement carte uniquement, 
        /// 									= C → traitement chèque (si pas d'appli chèque, message "FONCTIONNON IMPLEMENTEE ") 
        /// 									= autre → choix "CARTE CHEQUE"
        /// xx...55..xx	: REP		: si l’application, ayant traitée le débit, a renseigné le champ REP, celui-ci est remonté.
        /// xxx			: DEVISE	: Code numérique de la devise
        ///									(pour info: 250 pour FRF, 978 pour EUR)
        ///									= code numérique connu → traitement du débit dans la devise spécifiée
        ///									= code numérique inconnu → affichage "FONCTION IMPOSSIBLE"
        /// 									= autre → choix "CARTE CHEQUE"
        /// xxxxxxxxxx	: PRIVE		: Données privées remontées par l’application.
        ///									par défaut = 10 caractères (à blanc lorsqu’elle ne véhicule rien
        ///									
        /// <param name="message"></param>
        /// <returns></returns>
        public PaymentResponse PrepareResponse(byte[] bytes) {

            ByteStream stream = new ByteStream(bytes);

            if(!stream.Available()) {
                throw new ProtocolException("Terminal response is empty");
            }

            // at least STX ETX LRC
            if(!stream.Available(3)) {
                throw new ProtocolException("Laformed terminal response : " + BitConverter.ToString(bytes));
            }

            // Must start with STX
            if(stream.ReadFirstByte() != STX) {
                throw new ProtocolException("Response does not start with ETX : " + BitConverter.ToString(bytes));
            }

            // Read payload and compute LRC
            List<byte> payload = new List<byte>();

            stream.ReadBytesUntil(ETX, true, payload);
            int lrc = stream.ReadLastByte();
            int lrcExpected = LRC(payload.ToArray());

            if (lrcExpected != lrc) {
                throw new ProtocolException(String.Format("Wrong LRC for {0} Expected {1}, Given {2} ", BitConverter.ToString(payload.ToArray()), lrcExpected, lrc));
            }

            // now parse payload, reset the stream first with payload
            stream.Close();
            stream.Dispose();

            payload.RemoveAt(payload.Count - 1); // Delete ETX
            stream = new ByteStream(payload.ToArray());

            PaymentResponse response = new PaymentResponse();
            response.Pos = stream.ReadStringToShort(2);
            response.Status = stream.ReadChar();
            response.Amount = (float)stream.ReadStringToInt(8) / 100;
            response.Mode = stream.ReadChar();

            // If content is available
            if (stream.BytesToRead() > 13) {
                response.Content = stream.ReadString(55);
            }

            response.Currency = stream.ReadStringToShort(3);
            response.Data = stream.ReadString(10);

            stream.Close();
            stream.Dispose();

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public override Issue SendPaymentRequest(PaymentRequest request) {
            byte[] bytes = PrepareMessage(request);

            // Step 1
            Terminal.Connect();
            Terminal.Write(ENQ);

            if (Terminal.ReadOne() != ACK) {
                Terminal.Disconnect();
                return Issue.Failure("Terminal did not respond ACK after login");
            }

            Terminal.Write(bytes);

            if (Terminal.ReadOne() != ACK) {
                Terminal.Disconnect();
                return Issue.Failure("Terminal did not respond ACK after payment request");
            }

            Terminal.Write(EOT);

            // Step 2
            if (!Terminal.WaitData(WAITING_TIME)) {
                Terminal.Disconnect();
                return Issue.Failure("Terminal response was empty");
            }

            if (Terminal.ReadOne() != ENQ) {
                Terminal.Disconnect();
                return Issue.Failure("Terminal did not respond ENQ after payment waiting");
            }

            Terminal.Write(ACK);

            Terminal.WaitData(WAITING_TIME);
            bytes = Terminal.ReadAll();

            Terminal.Write(ACK);

            Terminal.Disconnect();

            PaymentResponse response = null;

            try {
                response = PrepareResponse(bytes);
            } catch (Exception e) {
                return Issue.Failure(e.Message);
            }

            if(response.Status == ConcertProtocol.STATUS_SUCCESS) {
                return Issue.Success("Payment success", response);
            } else {
                return Issue.Failure("Payment failure", response);
            }

        }

    }

}
