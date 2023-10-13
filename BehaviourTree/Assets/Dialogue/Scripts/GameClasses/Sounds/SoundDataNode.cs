using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dialogue.Scripts.GameClasses.Sounds
{
    [XmlRoot(ElementName="AudioSource")]
    public class AudioSource { 

        [XmlAttribute(AttributeName="name")] 
        public string Name { get; set; } 
    }

    [XmlRoot(ElementName="AudioClip")]
    public class AudioClip { 

        [XmlAttribute(AttributeName="ClipName")] 
        public string ClipName { get; set; } 

        [XmlAttribute(AttributeName="Loop")] 
        public bool Loop { get; set; } 
    }

    [XmlRoot(ElementName="LocalCrouchVolumeScale")]
    public class LocalCrouchVolumeScale { 

        [XmlAttribute(AttributeName="value")] 
        public double Value { get; set; } 
    }

    [XmlRoot(ElementName="CrouchNoiseScale")]
    public class CrouchNoiseScale { 

        [XmlAttribute(AttributeName="value")] 
        public double Value { get; set; } 
    }

    [XmlRoot(ElementName="NoiseScale")]
    public class NoiseScale { 

        [XmlAttribute(AttributeName="value")] 
        public int Value { get; set; } 
    }

    [XmlRoot(ElementName="MaxVoices")]
    public class MaxVoices { 

        [XmlAttribute(AttributeName="value")] 
        public int Value { get; set; } 
    }

    [XmlRoot(ElementName="SoundDataNode")]
    public class SoundDataNode
    {

        [XmlElement(ElementName = "AudioSource")]
        public AudioSource AudioSource { get; set; } = new AudioSource();

        [XmlElement(ElementName = "AudioClip")]
        public AudioClip AudioClip { get; set; } = new AudioClip();

        [XmlElement(ElementName = "LocalCrouchVolumeScale")]
        public LocalCrouchVolumeScale LocalCrouchVolumeScale { get; set; } = new LocalCrouchVolumeScale();

        [XmlElement(ElementName = "CrouchNoiseScale")]
        public CrouchNoiseScale CrouchNoiseScale { get; set; } = new CrouchNoiseScale();

        [XmlElement(ElementName = "NoiseScale")]
        public NoiseScale NoiseScale { get; set; } = new NoiseScale();

        [XmlElement(ElementName = "MaxVoices")]
        public MaxVoices MaxVoices { get; set; } = new MaxVoices();

        [XmlAttribute(AttributeName="name")] 
        public string Name { get; set; } 
    }
    
    [XmlRoot(ElementName="Sounds")]
    public class Sounds
    {

        [XmlElement(ElementName = "append")] public AppendSound soundAppend = new AppendSound();
    }
    
    public class AppendSound
    {
        [XmlAttribute(AttributeName = "xpath")] public string xpath = "/Sounds";
        [XmlElement("SoundDataNode")]
        public List<SoundDataNode> SoundDataNodes = new List<SoundDataNode>();

   }

}