public enum ActuatorType
{
    Rele1Canal = 0,          // Relé de 1 canal - pode ser usado para controlar dispositivos simples, como lâmpadas, ventiladores, etc.
    Rele2Canais = 1,         // Relé de 2 canais - pode controlar dois dispositivos independentes
    Rele4Canais = 2,         // Relé de 4 canais - ideal para controlar vários dispositivos simultaneamente, como um sistema de iluminação
    ServoMotor = 3,          // Servo motor - usado para controle de movimentos precisos em dispositivos mecânicos
    MotorDC = 4,             // Motor DC - utilizado em sistemas que exigem controle de velocidade e direção, como ventiladores e pequenos robôs
    MotorPassoAPasso = 5,    // Motor de passo a passo - utilizado em sistemas que exigem precisão no movimento, como impressoras 3D e CNCs
    Ventilador = 6,          // Ventilador - usado para controle de ventiladores em sistemas de ventilação ou ar condicionado
    FechaduraEletrica = 7,  // Fechadura elétrica - usada em sistemas de segurança, como controle de portas automáticas ou fechaduras inteligentes
    CortinaAutomatica = 8,  // Cortina automática - atuador para abrir e fechar cortinas ou persianas de maneira automatizada
    BombaDeAgua = 9,        // Bomba de água - usada para controlar bombas em sistemas de irrigação, cisternas ou aquários
    AlarmeDeSeguranca = 10, // Alarme de segurança - usado para disparar alarmes ou notificações em sistemas de segurança
    LuminariaInteligente = 11, // Luminária inteligente - controle de intensidade e cor das lâmpadas em um sistema de iluminação inteligente
    ChuveiroEletrico = 12,  // Chuveiro elétrico - usado para controlar o acionamento de chuveiros, com controle de temperatura e intensidade
    CompressorDeAr = 13,    // Compressor de ar - usado em sistemas industriais ou de climatização
    MotorHidraulico = 14,   // Motor hidráulico - usado em sistemas industriais que requerem controle de movimentação hidráulica
    PonteH = 15,            // Ponte H - usado para controlar motores DC com controle de direção e velocidade
}


public class Actuator
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public int OutputPin { get; set; }
    public ActuatorType TypeActuator { get; set; }
    public string EspId { get; set; }
    public bool IsDigital { get; set; }
}
