using System.Collections.Generic;

namespace IeltsTeachingAssistant.Models;

public class DescriptorPoint
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class BandDescriptor
{
    public int Band { get; set; }
    public List<DescriptorPoint> Points { get; set; } = new();
}

public class CriterionDescriptor
{
    public string CriterionKey { get; set; } = string.Empty;
    public string CriterionName { get; set; } = string.Empty;
    public List<BandDescriptor> Bands { get; set; } = new();
}

public static class IeltsDescriptors
{
    public static List<CriterionDescriptor> SpeakingDescriptors { get; } = new()
    {
        new CriterionDescriptor
        {
            CriterionKey = "FC",
            CriterionName = "Fluency & Coherence",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "S_FC_9_1", Text = "speaks fluently with only rare repetition or self-correction" },
                    new DescriptorPoint { Id = "S_FC_9_2", Text = "any hesitation is content-related rather than to find words or grammar" },
                    new DescriptorPoint { Id = "S_FC_9_3", Text = "speaks coherently with fully appropriate cohesive features" },
                    new DescriptorPoint { Id = "S_FC_9_4", Text = "develops topics fully and appropriately" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "S_FC_8_1", Text = "speaks fluently with only occasional repetition or self-correction" },
                    new DescriptorPoint { Id = "S_FC_8_2", Text = "hesitation is usually content-related and only rarely to search for language" },
                    new DescriptorPoint { Id = "S_FC_8_3", Text = "develops topics coherently and appropriately" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "S_FC_7_1", Text = "speaks at length without noticeable effort or loss of coherence" },
                    new DescriptorPoint { Id = "S_FC_7_2", Text = "may demonstrate language-related hesitation at times" },
                    new DescriptorPoint { Id = "S_FC_7_3", Text = "uses a range of connectives and discourse markers with some flexibility" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "S_FC_6_1", Text = "is willing to speak at length, though may lose coherence at times" },
                    new DescriptorPoint { Id = "S_FC_6_2", Text = "uses a range of connectives and discourse markers but not always appropriately" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "S_FC_5_1", Text = "usually maintains flow of speech but uses repetition, self-correction" },
                    new DescriptorPoint { Id = "S_FC_5_2", Text = "may over-use certain connectives and discourse markers" },
                    new DescriptorPoint { Id = "S_FC_5_3", Text = "produces simple speech fluently, but more complex communication causes fluency problems" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "LR",
            CriterionName = "Lexical Resource",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "S_LR_9_1", Text = "uses vocabulary with full flexibility and precision in all topics" },
                    new DescriptorPoint { Id = "S_LR_9_2", Text = "uses idiomatic language naturally and accurately" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "S_LR_8_1", Text = "uses a wide vocabulary resource readily and flexibly" },
                    new DescriptorPoint { Id = "S_LR_8_2", Text = "uses less common and idiomatic vocabulary skillfully" },
                    new DescriptorPoint { Id = "S_LR_8_3", Text = "paraphrases effectively as required" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "S_LR_7_1", Text = "uses vocabulary resource flexibly to discuss a variety of topics" },
                    new DescriptorPoint { Id = "S_LR_7_2", Text = "uses some less common and idiomatic vocabulary" },
                    new DescriptorPoint { Id = "S_LR_7_3", Text = "paraphrases effectively" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "S_LR_6_1", Text = "has a wide enough vocabulary to discuss topics at length" },
                    new DescriptorPoint { Id = "S_LR_6_2", Text = "generally paraphrases successfully" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "S_LR_5_1", Text = "manages to talk about familiar and unfamiliar topics but uses vocabulary with limited flexibility" },
                    new DescriptorPoint { Id = "S_LR_5_2", Text = "attempts to use paraphrase but with mixed success" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "GRA",
            CriterionName = "Grammatical Range & Accuracy",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "S_GRA_9_1", Text = "uses a full range of structures naturally and appropriately" },
                    new DescriptorPoint { Id = "S_GRA_9_2", Text = "produces consistently accurate structures apart from 'slips'" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "S_GRA_8_1", Text = "uses a wide range of structures flexibly" },
                    new DescriptorPoint { Id = "S_GRA_8_2", Text = "produces a majority of error-free sentences with only very occasional inappropriacies" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "S_GRA_7_1", Text = "uses a range of complex structures with some flexibility" },
                    new DescriptorPoint { Id = "S_GRA_7_2", Text = "frequently produces error-free sentences, though some grammatical mistakes persist" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "S_GRA_6_1", Text = "uses a mix of simple and complex structures, but with limited flexibility" },
                    new DescriptorPoint { Id = "S_GRA_6_2", Text = "may make frequent mistakes with complex structures" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "S_GRA_5_1", Text = "produces basic sentence forms with reasonable accuracy" },
                    new DescriptorPoint { Id = "S_GRA_5_2", Text = "uses a limited range of more complex structures, but these usually contain errors" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "PR",
            CriterionName = "Pronunciation",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "S_PR_9_1", Text = "uses a full range of pronunciation features with precision and subtlety" },
                    new DescriptorPoint { Id = "S_PR_9_2", Text = "is effortless to understand throughout; L1 accent has no effect on intelligibility" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "S_PR_8_1", Text = "uses a wide range of pronunciation features" },
                    new DescriptorPoint { Id = "S_PR_8_2", Text = "is easy to understand throughout; L1 accent has minimal effect on intelligibility" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "S_PR_7_1", Text = "shows all the positive features of Band 6 and some, but not all, of the positive features of Band 8" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "S_PR_6_1", Text = "uses a range of pronunciation features with mixed control" },
                    new DescriptorPoint { Id = "S_PR_6_2", Text = "can generally be understood throughout, though mispronunciation of individual words or sounds reduces clarity at times" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "S_PR_5_1", Text = "shows all the positive features of Band 4 and some, but not all, of the positive features of Band 6" }
                }}
            }
        }
    };

    public static List<CriterionDescriptor> WritingDescriptors { get; } = new()
    {
        new CriterionDescriptor
        {
            CriterionKey = "TR",
            CriterionName = "Task Response / Achievement",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "W_TR_9_1", Text = "fully addresses all parts of the task" },
                    new DescriptorPoint { Id = "W_TR_9_2", Text = "presents a fully developed position in answer to the question with relevant, fully extended and well supported ideas" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "W_TR_8_1", Text = "sufficiently addresses all parts of the task" },
                    new DescriptorPoint { Id = "W_TR_8_2", Text = "presents a well-developed response to the question with relevant, extended and supported ideas" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "W_TR_7_1", Text = "addresses all parts of the task" },
                    new DescriptorPoint { Id = "W_TR_7_2", Text = "presents a clear position throughout the response" },
                    new DescriptorPoint { Id = "W_TR_7_3", Text = "presents, extends and supports main ideas" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "W_TR_6_1", Text = "addresses all parts of the task although some parts may be more fully covered than others" },
                    new DescriptorPoint { Id = "W_TR_6_2", Text = "presents a relevant position although the conclusions may become unclear or repetitive" },
                    new DescriptorPoint { Id = "W_TR_6_3", Text = "presents relevant main ideas but some may be inadequately developed/unclear" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "W_TR_5_1", Text = "addresses the task only partially" },
                    new DescriptorPoint { Id = "W_TR_5_2", Text = "expresses a position but the development is not always clear and there may be no conclusions drawn" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "CC",
            CriterionName = "Coherence & Cohesion",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "W_CC_9_1", Text = "uses cohesion in such a way that it attracts no attention" },
                    new DescriptorPoint { Id = "W_CC_9_2", Text = "skillfully manages paragraphing" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "W_CC_8_1", Text = "sequences information and ideas logically" },
                    new DescriptorPoint { Id = "W_CC_8_2", Text = "manages all aspects of cohesion well" },
                    new DescriptorPoint { Id = "W_CC_8_3", Text = "uses paragraphing sufficiently and appropriately" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "W_CC_7_1", Text = "logically organizes information and ideas; there is clear progression throughout" },
                    new DescriptorPoint { Id = "W_CC_7_2", Text = "uses a range of cohesive devices appropriately although there may be some under-/over-use" },
                    new DescriptorPoint { Id = "W_CC_7_3", Text = "presents a clear central topic within each paragraph" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "W_CC_6_1", Text = "arranges information and ideas coherently and there is a clear overall progression" },
                    new DescriptorPoint { Id = "W_CC_6_2", Text = "uses cohesive devices effectively, but cohesion within and/or between sentences may be faulty or mechanical" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "W_CC_5_1", Text = "presents information with some organization but there may be a lack of overall progression" },
                    new DescriptorPoint { Id = "W_CC_5_2", Text = "makes inadequate, inaccurate or over-use of cohesive devices" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "LR",
            CriterionName = "Lexical Resource",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "W_LR_9_1", Text = "uses a wide range of vocabulary with very natural and sophisticated control of lexical features" },
                    new DescriptorPoint { Id = "W_LR_9_2", Text = "rare minor errors occur only as 'slips'" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "W_LR_8_1", Text = "uses a wide range of vocabulary fluently and flexibly to convey precise meanings" },
                    new DescriptorPoint { Id = "W_LR_8_2", Text = "skillfully uses uncommon lexical items but there may be occasional inaccuracies in word choice and collocation" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "W_LR_7_1", Text = "uses a sufficient range of vocabulary to allow some flexibility and precision" },
                    new DescriptorPoint { Id = "W_LR_7_2", Text = "uses less common lexical items with some awareness of style and collocation" },
                    new DescriptorPoint { Id = "W_LR_7_3", Text = "may produce occasional errors in word choice, spelling and/or word formation" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "W_LR_6_1", Text = "uses an adequate range of vocabulary for the task" },
                    new DescriptorPoint { Id = "W_LR_6_2", Text = "attempts to use less common vocabulary but with some inaccuracy" },
                    new DescriptorPoint { Id = "W_LR_6_3", Text = "makes some errors in spelling and/or word formation, but they do not impede communication" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "W_LR_5_1", Text = "uses a limited range of vocabulary, but this is minimally adequate for the task" },
                    new DescriptorPoint { Id = "W_LR_5_2", Text = "may make noticeable errors in spelling and/or word formation that may cause some difficulty for the reader" }
                }}
            }
        },
        new CriterionDescriptor
        {
            CriterionKey = "GRA",
            CriterionName = "Grammatical Range & Accuracy",
            Bands = new List<BandDescriptor>
            {
                new BandDescriptor { Band = 9, Points = new() {
                    new DescriptorPoint { Id = "W_GRA_9_1", Text = "uses a wide range of structures with full flexibility and accuracy" },
                    new DescriptorPoint { Id = "W_GRA_9_2", Text = "rare minor errors occur only as 'slips'" }
                }},
                new BandDescriptor { Band = 8, Points = new() {
                    new DescriptorPoint { Id = "W_GRA_8_1", Text = "uses a wide range of structures" },
                    new DescriptorPoint { Id = "W_GRA_8_2", Text = "the majority of sentences are error-free" },
                    new DescriptorPoint { Id = "W_GRA_8_3", Text = "makes only very occasional errors or inappropriacies" }
                }},
                new BandDescriptor { Band = 7, Points = new() {
                    new DescriptorPoint { Id = "W_GRA_7_1", Text = "uses a variety of complex structures" },
                    new DescriptorPoint { Id = "W_GRA_7_2", Text = "produces frequent error-free sentences" },
                    new DescriptorPoint { Id = "W_GRA_7_3", Text = "has good control of grammar and punctuation but may make a few errors" }
                }},
                new BandDescriptor { Band = 6, Points = new() {
                    new DescriptorPoint { Id = "W_GRA_6_1", Text = "uses a mix of simple and complex sentence forms" },
                    new DescriptorPoint { Id = "W_GRA_6_2", Text = "makes some errors in grammar and punctuation but they rarely reduce communication" }
                }},
                new BandDescriptor { Band = 5, Points = new() {
                    new DescriptorPoint { Id = "W_GRA_5_1", Text = "uses only a limited range of structures" },
                    new DescriptorPoint { Id = "W_GRA_5_2", Text = "attempts complex sentences but these tend to be less accurate than simple sentences" },
                    new DescriptorPoint { Id = "W_GRA_5_3", Text = "may make frequent grammatical errors and punctuation may be faulty" }
                }}
            }
        }
    };
}
