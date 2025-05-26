using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetupWithWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Piranha_Blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IsReusable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Blocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentGroups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Group = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 6, nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_MediaFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_MediaFolders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PageTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Params",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Params", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_SiteTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_SiteTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Taxonomies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Taxonomies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_BlockFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_BlockFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_BlockFields_Piranha_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Piranha_Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Sites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SiteTypeId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    InternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    LogoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Hostnames = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 6, nullable: true),
                    ContentLastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Sites_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    FolderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    AltText = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    PublicUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Media_Piranha_MediaFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Piranha_MediaFolders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TypeId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PrimaryImageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Excerpt = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Content", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Content_Piranha_ContentTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Piranha_ContentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_Content_Piranha_Taxonomies_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Piranha_Taxonomies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Piranha_WorkflowStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StateId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsInitial = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorCode = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_WorkflowStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_WorkflowStates_Piranha_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "Piranha_WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AliasUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RedirectUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Aliases_Piranha_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Piranha_Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageTypeId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, defaultValue: "Page"),
                    PrimaryImageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Excerpt = table.Column<string>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    NavigationTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    RedirectUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RedirectType = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableComments = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CloseCommentsAfterDays = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalPageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MetaIndex = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: true),
                    MetaFollow = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: true),
                    MetaPriority = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.5),
                    OgTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    OgDescription = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OgImageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Published = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Pages_Piranha_PageTypes_PageTypeId",
                        column: x => x.PageTypeId,
                        principalTable: "Piranha_PageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_Pages_Piranha_Pages_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Piranha_Pages_Piranha_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Piranha_Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_SiteFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_SiteFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_SiteFields_Piranha_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Piranha_Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_MediaVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    FileExtension = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_MediaVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_MediaVersions_Piranha_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Piranha_Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentBlocks_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFields_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentTaxonomies",
                columns: table => new
                {
                    ContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaxonomyId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentTaxonomies", x => new { x.ContentId, x.TaxonomyId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTaxonomies_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTaxonomies_Piranha_Taxonomies_TaxonomyId",
                        column: x => x.TaxonomyId,
                        principalTable: "Piranha_Taxonomies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentTranslations",
                columns: table => new
                {
                    ContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Excerpt = table.Column<string>(type: "TEXT", nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentTranslations", x => new { x.ContentId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTranslations_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTranslations_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_TransitionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CommentTemplate = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RequiresComment = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedRoles = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FromStateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToStateId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_TransitionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_TransitionRules_Piranha_WorkflowStates_FromStateId",
                        column: x => x.FromStateId,
                        principalTable: "Piranha_WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_TransitionRules_Piranha_WorkflowStates_ToStateId",
                        column: x => x.ToStateId,
                        principalTable: "Piranha_WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ContentTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentStateId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_WorkflowInstances_Piranha_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "Piranha_WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Piranha_WorkflowInstances_Piranha_WorkflowStates_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "Piranha_WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Categories_Piranha_Pages_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PageBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PageBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PageBlocks_Piranha_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Piranha_Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_PageBlocks_Piranha_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PageComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PageComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PageComments_Piranha_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PageFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PageFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PageFields_Piranha_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PagePermissions",
                columns: table => new
                {
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Permission = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PagePermissions", x => new { x.PageId, x.Permission });
                    table.ForeignKey(
                        name: "FK_Piranha_PagePermissions_Piranha_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PageRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PageRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PageRevisions_Piranha_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Tags_Piranha_Pages_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentBlockFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentBlockFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentBlockFields_Piranha_ContentBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Piranha_ContentBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentFieldTranslations",
                columns: table => new
                {
                    FieldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentFieldTranslations", x => new { x.FieldId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFieldTranslations_Piranha_ContentFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Piranha_ContentFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFieldTranslations_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_WorkflowContentExtensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CurrentWorkflowInstanceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsInWorkflow = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastWorkflowState = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_WorkflowContentExtensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_WorkflowContentExtensions_Piranha_WorkflowInstances_CurrentWorkflowInstanceId",
                        column: x => x.CurrentWorkflowInstanceId,
                        principalTable: "Piranha_WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostTypeId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    BlogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrimaryImageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Excerpt = table.Column<string>(type: "TEXT", nullable: true),
                    RedirectUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RedirectType = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableComments = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CloseCommentsAfterDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MetaIndex = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: true),
                    MetaFollow = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: true),
                    MetaPriority = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.5),
                    OgTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    OgDescription = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OgImageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Published = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_Posts_Piranha_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Piranha_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Piranha_Posts_Piranha_Pages_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Piranha_Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_Posts_Piranha_PostTypes_PostTypeId",
                        column: x => x.PostTypeId,
                        principalTable: "Piranha_PostTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentBlockFieldTranslations",
                columns: table => new
                {
                    FieldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentBlockFieldTranslations", x => new { x.FieldId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentBlockFieldTranslations_Piranha_ContentBlockFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Piranha_ContentBlockFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentBlockFieldTranslations_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PostBlocks_Piranha_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Piranha_Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_PostBlocks_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Author = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PostComments_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FieldId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CLRType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PostFields_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostPermissions",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Permission = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostPermissions", x => new { x.PostId, x.Permission });
                    table.ForeignKey(
                        name: "FK_Piranha_PostPermissions_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_PostRevisions_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_PostTags",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_PostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_Piranha_PostTags_Piranha_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Piranha_Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_PostTags_Piranha_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Piranha_Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Aliases_SiteId_AliasUrl",
                table: "Piranha_Aliases",
                columns: new[] { "SiteId", "AliasUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_BlockFields_BlockId_FieldId_SortOrder",
                table: "Piranha_BlockFields",
                columns: new[] { "BlockId", "FieldId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Categories_BlogId_Slug",
                table: "Piranha_Categories",
                columns: new[] { "BlogId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Content_CategoryId",
                table: "Piranha_Content",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Content_TypeId",
                table: "Piranha_Content",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentBlockFields_BlockId_FieldId_SortOrder",
                table: "Piranha_ContentBlockFields",
                columns: new[] { "BlockId", "FieldId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentBlockFieldTranslations_LanguageId",
                table: "Piranha_ContentBlockFieldTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentBlocks_ContentId",
                table: "Piranha_ContentBlocks",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentFields_ContentId_RegionId_FieldId_SortOrder",
                table: "Piranha_ContentFields",
                columns: new[] { "ContentId", "RegionId", "FieldId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentFieldTranslations_LanguageId",
                table: "Piranha_ContentFieldTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentTaxonomies_TaxonomyId",
                table: "Piranha_ContentTaxonomies",
                column: "TaxonomyId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentTranslations_LanguageId",
                table: "Piranha_ContentTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Media_FolderId",
                table: "Piranha_Media",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_MediaVersions_MediaId_Width_Height",
                table: "Piranha_MediaVersions",
                columns: new[] { "MediaId", "Width", "Height" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PageBlocks_BlockId",
                table: "Piranha_PageBlocks",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PageBlocks_PageId_SortOrder",
                table: "Piranha_PageBlocks",
                columns: new[] { "PageId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PageComments_PageId",
                table: "Piranha_PageComments",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PageFields_PageId_RegionId_FieldId_SortOrder",
                table: "Piranha_PageFields",
                columns: new[] { "PageId", "RegionId", "FieldId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PageRevisions_PageId",
                table: "Piranha_PageRevisions",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Pages_PageTypeId",
                table: "Piranha_Pages",
                column: "PageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Pages_ParentId",
                table: "Piranha_Pages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Pages_SiteId_Slug",
                table: "Piranha_Pages",
                columns: new[] { "SiteId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Params_Key",
                table: "Piranha_Params",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostBlocks_BlockId",
                table: "Piranha_PostBlocks",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostBlocks_PostId_SortOrder",
                table: "Piranha_PostBlocks",
                columns: new[] { "PostId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostComments_PostId",
                table: "Piranha_PostComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostFields_PostId_RegionId_FieldId_SortOrder",
                table: "Piranha_PostFields",
                columns: new[] { "PostId", "RegionId", "FieldId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostRevisions_PostId",
                table: "Piranha_PostRevisions",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Posts_BlogId_Slug",
                table: "Piranha_Posts",
                columns: new[] { "BlogId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Posts_CategoryId",
                table: "Piranha_Posts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Posts_PostTypeId",
                table: "Piranha_Posts",
                column: "PostTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_PostTags_TagId",
                table: "Piranha_PostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_SiteFields_SiteId_RegionId_FieldId_SortOrder",
                table: "Piranha_SiteFields",
                columns: new[] { "SiteId", "RegionId", "FieldId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Sites_InternalId",
                table: "Piranha_Sites",
                column: "InternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Sites_LanguageId",
                table: "Piranha_Sites",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Tags_BlogId_Slug",
                table: "Piranha_Tags",
                columns: new[] { "BlogId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Taxonomies_GroupId_Type_Slug",
                table: "Piranha_Taxonomies",
                columns: new[] { "GroupId", "Type", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_TransitionRules_FromStateId",
                table: "Piranha_TransitionRules",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_TransitionRules_FromStateId_ToStateId",
                table: "Piranha_TransitionRules",
                columns: new[] { "FromStateId", "ToStateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_TransitionRules_IsActive",
                table: "Piranha_TransitionRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_TransitionRules_ToStateId",
                table: "Piranha_TransitionRules",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowContentExtensions_ContentId",
                table: "Piranha_WorkflowContentExtensions",
                column: "ContentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowContentExtensions_ContentType",
                table: "Piranha_WorkflowContentExtensions",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowContentExtensions_CurrentWorkflowInstanceId",
                table: "Piranha_WorkflowContentExtensions",
                column: "CurrentWorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowContentExtensions_IsInWorkflow",
                table: "Piranha_WorkflowContentExtensions",
                column: "IsInWorkflow");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowDefinitions_Created",
                table: "Piranha_WorkflowDefinitions",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowDefinitions_IsActive",
                table: "Piranha_WorkflowDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowDefinitions_Name",
                table: "Piranha_WorkflowDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_ContentId",
                table: "Piranha_WorkflowInstances",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_ContentId_Status",
                table: "Piranha_WorkflowInstances",
                columns: new[] { "ContentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_CreatedBy",
                table: "Piranha_WorkflowInstances",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_CurrentStateId",
                table: "Piranha_WorkflowInstances",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_LastModified",
                table: "Piranha_WorkflowInstances",
                column: "LastModified");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_Status",
                table: "Piranha_WorkflowInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowInstances_WorkflowDefinitionId",
                table: "Piranha_WorkflowInstances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_IsInitial",
                table: "Piranha_WorkflowStates",
                columns: new[] { "WorkflowDefinitionId", "IsInitial" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_IsPublished",
                table: "Piranha_WorkflowStates",
                columns: new[] { "WorkflowDefinitionId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_SortOrder",
                table: "Piranha_WorkflowStates",
                columns: new[] { "WorkflowDefinitionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_StateId",
                table: "Piranha_WorkflowStates",
                columns: new[] { "WorkflowDefinitionId", "StateId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Piranha_Aliases");

            migrationBuilder.DropTable(
                name: "Piranha_BlockFields");

            migrationBuilder.DropTable(
                name: "Piranha_ContentBlockFieldTranslations");

            migrationBuilder.DropTable(
                name: "Piranha_ContentFieldTranslations");

            migrationBuilder.DropTable(
                name: "Piranha_ContentGroups");

            migrationBuilder.DropTable(
                name: "Piranha_ContentTaxonomies");

            migrationBuilder.DropTable(
                name: "Piranha_ContentTranslations");

            migrationBuilder.DropTable(
                name: "Piranha_MediaVersions");

            migrationBuilder.DropTable(
                name: "Piranha_PageBlocks");

            migrationBuilder.DropTable(
                name: "Piranha_PageComments");

            migrationBuilder.DropTable(
                name: "Piranha_PageFields");

            migrationBuilder.DropTable(
                name: "Piranha_PagePermissions");

            migrationBuilder.DropTable(
                name: "Piranha_PageRevisions");

            migrationBuilder.DropTable(
                name: "Piranha_Params");

            migrationBuilder.DropTable(
                name: "Piranha_PostBlocks");

            migrationBuilder.DropTable(
                name: "Piranha_PostComments");

            migrationBuilder.DropTable(
                name: "Piranha_PostFields");

            migrationBuilder.DropTable(
                name: "Piranha_PostPermissions");

            migrationBuilder.DropTable(
                name: "Piranha_PostRevisions");

            migrationBuilder.DropTable(
                name: "Piranha_PostTags");

            migrationBuilder.DropTable(
                name: "Piranha_SiteFields");

            migrationBuilder.DropTable(
                name: "Piranha_SiteTypes");

            migrationBuilder.DropTable(
                name: "Piranha_TransitionRules");

            migrationBuilder.DropTable(
                name: "Piranha_WorkflowContentExtensions");

            migrationBuilder.DropTable(
                name: "Piranha_ContentBlockFields");

            migrationBuilder.DropTable(
                name: "Piranha_ContentFields");

            migrationBuilder.DropTable(
                name: "Piranha_Media");

            migrationBuilder.DropTable(
                name: "Piranha_Blocks");

            migrationBuilder.DropTable(
                name: "Piranha_Posts");

            migrationBuilder.DropTable(
                name: "Piranha_Tags");

            migrationBuilder.DropTable(
                name: "Piranha_WorkflowInstances");

            migrationBuilder.DropTable(
                name: "Piranha_ContentBlocks");

            migrationBuilder.DropTable(
                name: "Piranha_MediaFolders");

            migrationBuilder.DropTable(
                name: "Piranha_Categories");

            migrationBuilder.DropTable(
                name: "Piranha_PostTypes");

            migrationBuilder.DropTable(
                name: "Piranha_WorkflowStates");

            migrationBuilder.DropTable(
                name: "Piranha_Content");

            migrationBuilder.DropTable(
                name: "Piranha_Pages");

            migrationBuilder.DropTable(
                name: "Piranha_WorkflowDefinitions");

            migrationBuilder.DropTable(
                name: "Piranha_ContentTypes");

            migrationBuilder.DropTable(
                name: "Piranha_Taxonomies");

            migrationBuilder.DropTable(
                name: "Piranha_PageTypes");

            migrationBuilder.DropTable(
                name: "Piranha_Sites");

            migrationBuilder.DropTable(
                name: "Piranha_Languages");
        }
    }
}
