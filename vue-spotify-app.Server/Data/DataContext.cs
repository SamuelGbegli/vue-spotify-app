using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;

namespace vue_spotify_app.Server.Data
{
    public class DataContext : DbContext
    {
        /// <summary>
        /// Gets or sets the collection of albums in the database context.
        /// </summary>
        /// <remarks>Use this property to query, add, update, or remove albums from the underlying
        /// database. Changes made to the collection are tracked by the context and can be persisted to the database
        /// using SaveChanges().</remarks>
        public DbSet<Album> Albums { get; set; }
        /// <summary>
        /// Gets or sets the collection of artists in the database context.
        /// </summary>
        /// <remarks>This property provides access to query, add, update, or remove artist entities using
        /// Entity Framework Core. Changes made to this collection are tracked by the context and persisted to the
        /// database when SaveChanges is called.</remarks>
        public DbSet<Artist> Artists { get; set; }
        /// <summary>
        /// Gets or sets the collection of album cover entities in the database context.
        /// </summary>
        public DbSet<AlbumCover> AlbumCovers { get; set; }
        /// <summary>
        /// Gets or sets the collection of tracks in the database context.
        /// </summary>
        /// <remarks>This property provides access to query, add, update, or remove track entities using
        /// Entity Framework Core. Changes made to the collection are tracked and persisted to the database when
        /// SaveChanges is called.</remarks>
        public DbSet<Track> Tracks { get; set; }

        public DbSet<Playlist> Playlists { get; set; }
        /// <summary>
        /// Gets or sets the collection of track records in the database.
        /// </summary>
        /// <remarks>This property provides access to query, add, update, or remove track records using
        /// Entity Framework Core. Changes made to the collection are tracked and persisted to the database when
        /// SaveChanges is called.</remarks>
        public DbSet<TrackRecord> TrackRecords { get; set; }
        /// <summary>
        /// Gets or sets the collection of playback records in the database.
        /// </summary>
        public DbSet<PlaybackRecord> PlaybackRecords { get; set; }
        /// <summary>
        /// Gets or sets the collection of pending playback records in the database.
        /// </summary>
        public DbSet<PendingPlaybackRecord> PendingPlaybackRecords { get; set; }

        public DbSet<SpotifyToken> SpotifyTokens { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<TrackAlias> TrackAliases { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>()
                .ToTable("Albums")
                .HasMany(a => a.Tracks)
                .WithOne(t => t.Album)
                .HasForeignKey(t => t.AlbumID);

            modelBuilder.Entity<Album>()
                .HasOne(a => a.AlbumCover)
                .WithOne(ac => ac.Album)
                .HasForeignKey<AlbumCover>(ac => ac.AlbumID);

            modelBuilder.Entity<Album>()
                .HasMany(a => a.Artists)
                .WithMany(a => a.Albums)
                .UsingEntity<Dictionary<string, object>>(
                "AlbumArtist",
                joinRight => joinRight
                .HasOne<Artist>()
                .WithMany()
                .HasForeignKey("ArtistID")
                .HasConstraintName("FK_AlbumArtist_Artists_ArtistID")
                .OnDelete(DeleteBehavior.Cascade),
                joinLeft => joinLeft
                .HasOne<Album>()
                .WithMany()
                .HasForeignKey("AlbumID")
                .HasConstraintName("FK_AlbumArtist_Albums_AlbumID")
                 .OnDelete(DeleteBehavior.Cascade),
                                joinEntity =>
                                {
                                    joinEntity.Property<string>("ID").ValueGeneratedOnAdd();
                                    joinEntity.HasKey("ID");
                                    joinEntity.HasIndex(["AlbumID", "ArtistID"]).IsUnique();
                                    joinEntity.ToTable("AlbumArtist");
                                    joinEntity.HasIndex("ArtistID");
                                })
                ;

            modelBuilder.Entity<Artist>()
                .ToTable("Artists")
                .HasMany(a => a.Albums)
                .WithMany(a => a.Artists);

            modelBuilder.Entity<Artist>()
                .HasMany(a => a.Tracks)
                .WithMany(t => t.Artists)
                .UsingEntity<Dictionary<string, object>>(
                "ArtistTrack",
                joinRight => joinRight
                .HasOne<Track>()
                .WithMany()
                .HasForeignKey("TrackID")
                .HasConstraintName("FK_ArtistTrack_Tracks_TrackID")
                .OnDelete(DeleteBehavior.Cascade),
                joinLeft => joinLeft
                .HasOne<Artist>()
                .WithMany()
                .HasForeignKey("ArtistID")
                .HasConstraintName("FK_ArtistTrack_Artists_ArtistID")
                .OnDelete(DeleteBehavior.Cascade),
                joinEntity =>
                {
                    joinEntity.Property<string>("ID").ValueGeneratedOnAdd();
                    joinEntity.HasKey("ID");

                    joinEntity.HasIndex(["ArtistID", "TrackID"]).IsUnique();

                    joinEntity.ToTable("ArtistTrack");
                    joinEntity.HasIndex("TrackID");
                });


            modelBuilder.Entity<AlbumCover>()
                .ToTable("AlbumCovers")
                .HasKey("ID");

            modelBuilder.Entity<Track>()
                .ToTable("Tracks")
                .HasMany(t => t.Artists)
                .WithMany(a => a.Tracks);

            modelBuilder.Entity<TrackRecord>()
                .ToTable("TrackRecords");

            modelBuilder.Entity<PlaybackRecord>()
                .ToTable("PlaybackRecords");

            modelBuilder.Entity<PendingPlaybackRecord>()
                .ToTable("PendingPlaybackRecords");

            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasOne(u => u.SpotifyToken)
                .WithOne(st => st.User)
                .HasForeignKey<SpotifyToken>(st => st.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrackAlias>()
                .ToTable("TrackAliases")
                .HasMany(a => a.Tracks)
                .WithOne(t => t.Alias)
                .HasForeignKey(t => t.AliasID);
        }

    }
}
