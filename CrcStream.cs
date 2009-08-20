using System;
using System.IO;

namespace Weland {
    public class CrcStream : Stream {
	public CrcStream(Stream s) {
	    stream = s;
	    BuildTable();
	}

	public uint GetCRC() {
	    return (crc ^ 0xffffffff);
	}

	public override void Write(byte[] buffer, int offset, int count) {
	    for (int i = offset; i < offset + count; ++i) {
		uint a = (crc >> 8) & 0x00ffffff;
		uint b = (table[(crc ^ buffer[i]) & 0xff]);
		crc = a^b;
	    }
	    stream.Write(buffer, offset, count);
	}

	public Stream Stream {
	    get {
		return stream;
	    }
	}

	public override bool CanRead {
	    get {
		return false;
	    }
	}

	public override bool CanWrite {
	    get {
		return true;
	    }
	}

	public override bool CanSeek {
	    get {
		return false;
	    }
	}

	public override long Seek(long position, SeekOrigin origin) {
	    throw new NotSupportedException();
	}

	public override long Length {
	    get {
		throw new NotSupportedException();
	    }
	}

	public override long Position {
	    get {
		throw new NotSupportedException();
	    }
	    
	    set {
		throw new NotSupportedException();
	    }
	}

	public override void Flush() {
	    stream.Flush();
	}

	public override void SetLength(long length) {
	    throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count) {
	    throw new NotSupportedException();
	}

	void BuildTable() {
	    table = new uint[256];
	    for (int i = 0; i < table.Length; ++i) {
		uint crc = (uint) i;
		for (int j = 0; j < 8; ++j) {
		    if ((crc & 1) != 0) 
			crc = (crc >> 1) ^ polynomial;
		    else
			crc >>= 1;
		}
		table[i] = crc;
	    }
	}
	
	uint[] table;
	uint crc = 0xffffffff;
	const uint polynomial = 0xedb88320;
	Stream stream;
    }
}