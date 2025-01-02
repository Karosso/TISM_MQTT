# Serviço IoT - Smart Home Control

## Visão Geral
Sistema de gerenciamento IoT para dispositivos ESP32, sensores e atuadores. Inclui comunicação MQTT e sistema de alertas para monitoramento de sensores. Utiliza Firebase como banco de dados para armazenamento de dispositivos e histórico de medições. O sistema faz parte de um ecossistema maior de automação residencial, integrando-se com um aplicativo mobile em Flutter e o esp32, que é responsável pela coneção com o MQTT e leitura dos sensores e gerenciamento dos atuadores.

## 🌐 Ecossistema do Projeto

Este repositório é parte de um sistema maior de automação residencial que inclui:

- 📱 **Aplicativo Mobile** (Flutter): [smart_home_control](https://github.com/Karosso/smart_home_control)
- 🖥️ **Backend** (C#): Este repositório - CRUD e serviço MQTT
- 🔥 **Firebase**: Armazenamento de dados e autenticação
- 🔌 **ESP32** (C++): [smh_esp](https://github.com/Karosso/smh_esp)

## Arquitetura

### Componentes Principais

#### Controllers
1. **ActuatorController**
   - Gerencia operações CRUD para atuadores
   - Associa atuadores aos dispositivos ESP32

2. **SensorController**
   - Gerencia operações CRUD para sensores
   - Associa sensores aos dispositivos ESP32

3. **EspController**
   - Gerencia operações CRUD para dispositivos ESP32
   - Controla registro e gerenciamento de dispositivos
   - Mantém relações entre dispositivos e seus sensores/atuadores

4. **MqttController**
   - Gerencia publicação e assinatura de tópicos MQTT
   - Controla roteamento de mensagens entre serviço e dispositivos

5. **AlertController**
   - Cria e gerencia alertas dos sensores
   - Define regras de monitoramento baseadas em limites
   - Dispara notificações baseadas em valores dos sensores

#### Serviços

**MqttClientService**
- Monitora tópicos MQTT específicos
- Processa dados recebidos de sensores e atuadores
- Persiste dados no Firebase
- Gerencia comunicação eficiente entre dispositivos e o sistema.


**Estrutura de Tópicos MQTT**

A comunicação MQTT entre dispositivos e o serviço é feita utilizando dados enviados por json e utiliza a seguinte estrutura de tópicos:

- Dados dos sensores:
  - Tópico: `device/{macAddress}/sensors_data`
  - Descrição: Dados coletados pelos sensores associados a um dispositivo ESP32, serão salvos no Firebase

  ```csharp
  {
    "SensorId": "s_01",
    "Value": "41",
    "Timestamp": "2024-11-23T14:46:10Z"
  }
  ```

- Dados dos atuadores:
  - Tópico: `device/{macAddress}/actuators_data`
  - Descrição: Comandos enviados para controlar os atuadores.

  ```csharp
  {
   "ActuatorId": "a_01",
   "Timestamp": "2024-11-23T14:45:10.000Z",
   "Command": "turnOff"
  }
  ```

## Modelos de Dados

### ESP32
```csharp
public class ESP32 
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string MacAddress { get; set; }
    public Dictionary<string, Sensor> Sensors { get; set; }
    public Dictionary<string, Actuator> Actuators { get; set; }
}
```

### Sensor
```csharp
public class Sensor 
{
    public string Id { get; set; }
    public string Name { get; set; }
    public SensorType Type { get; set; }
    public string EspId { get; set; }
    public SensorData? LastData { get; set; }
}
```

### Atuador
```csharp
public class Actuator 
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ActuatorType TypeActuator { get; set; }
    public string EspId { get; set; }
    public string? macAddress { get; set; }
    public ActuatorData? LastData { get; set; }
}
```

### Modelos de Dados
```csharp
public class SensorData 
{
    public string SensorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Value { get; set; }
}

public class ActuatorData 
{
    public string ActuatorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Command { get; set; }
}

public class Alert 
{
    public string SensorId { get; set; }
    public string SensorName { get; set; }
    public double Value { get; set; }
}
```

## Funcionalidades

### Gerenciamento de Dispositivos
- Registro e gerenciamento de ESP32
- Associação de sensores e atuadores
- Monitoramento de status e conectividade

### Operações de Sensores
- Adicionar, atualizar e remover sensores
- Monitoramento em tempo real
- Armazenamento de histórico

### Controle de Atuadores
- Adicionar, atualizar e remover atuadores
- Enviar comandos
- Monitorar estados

### Sistema de Alertas
- Configuração de alertas baseados em limites.
- Notificações automáticas ao exceder valores pré-definidos.
- Histórico detalhado de alertas.

### Comunicação MQTT
- Assinar tópicos de dados dos dispositivos
- Publicar comandos de controle
- Gerenciar streaming de dados em tempo real
- Controlar protocolos de comunicação

## Começando

### Pré-requisitos
- .NET Core SDK
- Broker MQTT (ex: Mosquitto)
- Conta Firebase e configuração
- Pacotes NuGet necessários

### Configuração
1. Configurar credenciais do Firebase
2. Configurar conexão com broker MQTT
3. Configurar aplicação no `appsettings.json`

### Executando o Serviço
1. Clonar repositório
2. Instalar dependências
3. Configurar aplicação
4. Executar serviço com `dotnet run`

## Documentação da API

### Endpoints ESP32
- `GET /api/esp` - Listar ESP32
- `POST /api/esp` - Registrar novo ESP32
- `DELETE /api/esp/{id}` - Remover ESP32
- `GET /api/esp/devices` - Listar todos sensores e atuadores daquele usuário
- `GET /api/esp/sensors` - Listar todos sensores daquele usuário

### Endpoints Sensor
- `POST /api/sensor` - Adicionar sensor
- `DELETE /api/sensor/{id}` - Remover sensor

### Endpoints Atuador
- `POST /api/actuator` - Adicionar atuador
- `DELETE /api/actuator/{id}` - Remover atuador

### Endpoints Alerta
- `GET /api/alert` - Listar alertas
- `POST /api/alert` - Criar alerta
- `PUT /api/alert/{id}` - Atualizar alerta
- `DELETE /api/alert/{id}` - Remover alerta

## Considerações de Segurança
- Implementar autenticação e autorização
- Securizar comunicação MQTT
- Proteger dados sensíveis de configuração
- Validar dados de entrada
- Tratar erros adequadamente

## Tratamento de Erros
- Implementar tratamento global de exceções
- Registrar erros e exceções
- Fornecer mensagens significativas
- Tratar problemas de conectividade
- Gerenciar timeouts de comunicação

## Monitoramento e Logging
- Monitorar performance do sistema
- Monitorar conectividade dos dispositivos
- Registrar dados de sensores e atuadores
- Rastrear disparos de alertas
- Monitorar comunicação MQTT

## Como Contribuir

1. Faça um Fork do projeto
2. Crie uma Branch para sua Feature (`git checkout -b feature/AmazingFeature`)
3. Faça o Commit de suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Faça o Push para a Branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença
Este projeto está licenciado sob a Licença MIT - veja abaixo o texto completo:

```
MIT License

Copyright (c) 2024 Oscar Dias (https://github.com/Karosso)
Copyright (c) 2024 Bruno Reis (https://github.com/brunohreis)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Suporte

- Em caso de dúvidas ou problemas, abra uma issue no repositório do projeto
- Para questões relacionadas ao aplicativo mobile, visite [smart_home_control](https://github.com/Karosso/smart_home_control)
- Para questões relacionadas ao ESP32, visite [smh_esp](https://github.com/Karosso/smh_esp)

## Observações Importantes

- A versão atual não possui o alerta inApp ou Push notification, o serviço já reconhece quando um valor está dentro da regra de alertas, mas como obtivemos dificuldades na identificação do usuário essas funcionalidades serão implementadas posteriormente.
