// See https://aka.ms/new-console-template for more information
using System.Drawing;
using System.Drawing.Imaging;

var roots = new[]
{
	@"C:\_Work-C\Git\s-box-rotten-tater\rotten_tater\Assets\textures\atlasmap",
};

var output = @"C:\_Work-C\Git\s-box-rotten-tater\rotten_tater\Assets\textures";

var supported = new[] { ".png", ".jpg", ".jpeg", ".bmp" };

var ask = false;

var files = roots.Where(Directory.Exists).SelectMany(root =>
	Directory.GetFiles( root, "*.*", SearchOption.AllDirectories )
			.Where( f => supported.Contains( Path.GetExtension( f ).ToLower() ) )
	)
	.ToList();

if ( files.Count == 0 )
{
	Console.WriteLine( "No valid images found." );
	return;
}

int cols = Math.Ceiling( Math.Sqrt( files.Count ) ) > 0 ? (int)Math.Ceiling( Math.Sqrt( files.Count ) ) : 1;
int rows = (int)Math.Ceiling((double)files.Count / cols);

int tileSize = ask ? int.Parse( Console.ReadLine() ) : 256;

int atlasWidth = cols * tileSize;
int atlasHeight = rows * tileSize;

using var atlas = new Bitmap( atlasWidth, atlasHeight );
using var g = Graphics.FromImage( atlas );

g.Clear( Color.Magenta ); // debug background

for ( int i = 0; i < files.Count; i++ )
{
	try
	{
		using var img = new Bitmap( files[i] );

		int x = (i % cols) * tileSize;
		int y = (i / cols) * tileSize;

		g.DrawImage( img, new Rectangle( x, y, tileSize, tileSize ) );
	}
	catch ( Exception e )
	{
		Console.WriteLine( $"Failed: {files[i]} - {e.Message}" );
	}
}

atlas.Save( Path.Combine( output, $"atlas-{DateTime.Now:yyyyMMddHHmmss}-{files.Count}-{rows}-{cols}.png" ), ImageFormat.Png );

Console.WriteLine( $"Atlas saved to: {output}" );
