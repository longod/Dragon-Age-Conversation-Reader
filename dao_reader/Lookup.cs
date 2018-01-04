// (c) hikami, aka longod
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace dao_reader {
    //DAOzNumList.xls�����ɁA���{�ꉻ�v���W�F�N�g�ɑΉ�����t�@�C���ԍ���\�����邽�߂̑Ή��e�[�u��
    [XmlType(@"dacr")]
    public class Lookup {
        //public Module module = new Module();

        public class Module {

            public class Set {
                public Set() { }
                public Set( string num, uint min, uint max ) {
                    file = num;
                    min_id = min;
                    max_id = max;
                }
                [XmlAttribute]
                public string file;
                [XmlAttribute]
                public uint min_id;
                [XmlAttribute]
                public uint max_id;
            }
            
            [XmlAttribute]
            public string name = @"singleplayer_en-us";
            [XmlElement( "pair" )]
            public List<Set> set = new List<Set>();

            // ���я��������ɂȂ��Ă��Ȃ��\���i�N�����M��Ȃ�����Ȃ����j
            // ���l����ƁAid��min,max�̃����W�ŊǗ������ׂ�����
        }

        
        [XmlElement( "module" )]
        public List<Module> module = new List<Module>();
    }

}
